using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Game.Data;
using Game.Factories;
using Game.Grid.Item;
using Game.Lane.Item;
using Game.Models;
using Game.Presenters;
using Game.Utils;
using Game.Views;
using UnityEngine;

namespace Game.Handlers
{
    public sealed class LaneUnitShootHandler
    {
        private const int MaxActiveOrbiters = 5;
        private const float LaunchMoveDuration = 0.4f;
        private const float LaunchJumpPower = 3f;
        private const float OrbitSpeed = 15f;
        private const float BallSpeed = 50f;
        private const float MinBallDuration = 0.05f;
        private const float MaxBallDuration = 0.2f;

        private readonly LanePresenter _lanePresenter;
        private readonly UnitSlotPresenter _unitSlotPresenter;
        private readonly ILaneModel _laneModel;
        private readonly IUnitSlotModel _unitSlotModel;
        private readonly IPixelGridModel _pixelGridModel;
        private readonly IPixelGridView _pixelGridView;
        private readonly IProjectileFactory _projectileFactory;
        private readonly ILaneUnitFactory _laneUnitFactory;
        private readonly IPixelCellFactory _pixelCellFactory;

        private readonly List<BaseLaneUnitObject> _activeOrbiters = new(MaxActiveOrbiters);

        private LaneUnitOrbitPath _orbitPath;
        private bool _isInitialized;

        public LaneUnitShootHandler(LanePresenter lanePresenter, UnitSlotPresenter unitSlotPresenter,
            ILaneModel laneModel, IUnitSlotModel unitSlotModel, IPixelGridModel pixelGridModel,
            IPixelGridView pixelGridView, IProjectileFactory projectileFactory,
            ILaneUnitFactory laneUnitFactory, IPixelCellFactory pixelCellFactory)
        {
            _lanePresenter = lanePresenter;
            _unitSlotPresenter = unitSlotPresenter;
            _laneModel = laneModel;
            _unitSlotModel = unitSlotModel;
            _pixelGridModel = pixelGridModel;
            _pixelGridView = pixelGridView;
            _projectileFactory = projectileFactory;
            _laneUnitFactory = laneUnitFactory;
            _pixelCellFactory = pixelCellFactory;

            _lanePresenter.OnFrontUnitTapped += OnFrontUnitTapped;
            _unitSlotPresenter.OnSlotUnitTapped += OnSlotUnitTapped;
            _pixelGridView.OnViewInitialized += OnPixelGridReady;
        }

        public void Dispose()
        {
            _lanePresenter.OnFrontUnitTapped -= OnFrontUnitTapped;
            _unitSlotPresenter.OnSlotUnitTapped -= OnSlotUnitTapped;
            _pixelGridView.OnViewInitialized -= OnPixelGridReady;

            _activeOrbiters.Clear();
        }

        private void OnPixelGridReady()
        {
            _orbitPath = LaneUnitOrbitPathGenerator.Generate(_pixelGridView);
            _isInitialized = true;
        }

        private void OnFrontUnitTapped(BaseLaneUnitObject unit)
        {
            if (!_laneModel.TryGetLaneIndexOf(unit, out int laneIndex)) return;
            if (!CanStartOrbit(unit)) return;

            _laneModel.RemoveFrontUnit(laneIndex);
            LaunchAndRunOrbitAsync(unit).Forget();
        }

        private void OnSlotUnitTapped(int slotIndex, BaseLaneUnitObject unit)
        {
            if (!CanStartOrbit(unit)) return;

            _unitSlotModel.RemoveUnit(slotIndex);
            LaunchAndRunOrbitAsync(unit).Forget();
        }

        private bool CanStartOrbit(BaseLaneUnitObject unit)
        {
            if (!_isInitialized) return false;
            if (_activeOrbiters.Count >= MaxActiveOrbiters) return false;
            if (_activeOrbiters.Contains(unit)) return false;
            return true;
        }

        private async UniTask LaunchAndRunOrbitAsync(BaseLaneUnitObject unit)
        {
            _activeOrbiters.Add(unit);
            unit.SetParent(_pixelGridView.OrbitRoot);

            int startIdx = _orbitPath.LaunchNodeIndex;
            var launchPos = _orbitPath.Nodes[startIdx].Position;

            await unit.MotionAnimation.JumpTo(launchPos, LaunchJumpPower, LaunchMoveDuration);

            bool aimLocked = ShouldStartWithInwardAim(unit);

            unit.OrbitAnimation.SetPathRotationImmediate(_orbitPath.Nodes[startIdx].PathYaw);
            if (aimLocked) unit.OrbitAnimation.SetAimImmediate();
            else unit.OrbitAnimation.ResetAimImmediate();

            await OrbitRunner.Run(unit, _orbitPath, startIdx, OrbitSpeed, aimLocked, HandleTriggerAtNode);

            HandleOrbitCompleted(unit);
        }

        private async UniTask<bool> HandleTriggerAtNode(BaseLaneUnitObject unit, int nodeIndex, bool aimLocked)
        {
            var node = _orbitPath.Nodes[nodeIndex];

            if (!OrbitLineSearcher.TryFindMatchingPixelOnLine(_pixelGridModel, node, unit.ColorId, out var coord))
            {
                if (!aimLocked) unit.OrbitAnimation.ResetAimImmediate();
                return aimLocked;
            }

            var pixel = _pixelGridModel.GetGridObject(coord);
            if (!pixel)
            {
                if (!aimLocked) unit.OrbitAnimation.ResetAimImmediate();
                return aimLocked;
            }

            _pixelGridModel.SetGridObject(coord, null);
            unit.ConsumeAmmo(1);

            if (!aimLocked) await unit.OrbitAnimation.AimAtPixel();

            FireAsync(unit, pixel).Forget();
            
            if (unit.Ammo <= 0)
                ReleaseUnit(unit).Forget();

            return true;
        }

        private void HandleOrbitCompleted(BaseLaneUnitObject unit)
        {
            _activeOrbiters.Remove(unit);
        }

        private async UniTask FireAsync(BaseLaneUnitObject unit, BasePixelCellObject pixel)
        {
            var ball = _projectileFactory.GetProjectile<BallProjectileObject>();
            var spawnPos = unit.ProjectileSpawnPoint.position;
            var targetPos = pixel.transform.position;
            float duration = Mathf.Clamp(Vector3.Distance(spawnPos, targetPos) / BallSpeed, MinBallDuration, MaxBallDuration);

            ball.transform.position = spawnPos;
            await ball.MoveAnimation.MoveTo(targetPos, duration);
            _projectileFactory.ReleaseProjectile(ball);

            await pixel.CellAnimation.PlayBounceAndScaleDown();
            _pixelCellFactory.ReleasePixelCell(pixel);
        }

        private async UniTask ReleaseUnit(BaseLaneUnitObject unit)
        {
            _activeOrbiters.Remove(unit);
            await unit.MotionAnimation.ScaleDown();
            _laneUnitFactory.ReleaseLaneUnit(unit);
            unit.MotionAnimation.ResetScale();
        }

        private bool ShouldStartWithInwardAim(BaseLaneUnitObject unit)
        {
            var startNode = _orbitPath.Nodes[_orbitPath.LaunchNodeIndex];
            return OrbitLineSearcher.TryFindMatchingPixelOnLine(_pixelGridModel, startNode, unit.ColorId, out _);
        }
    }
}