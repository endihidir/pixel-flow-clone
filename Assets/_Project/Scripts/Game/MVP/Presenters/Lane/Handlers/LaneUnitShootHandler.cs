using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Game.Data;
using Game.Factories;
using Game.Grid.Item;
using Game.Lane.Item;
using Game.Models;
using Game.Utils;
using Game.Views;
using UnityEngine;

namespace Game.Handlers
{
    public sealed class LaneUnitShootHandler : ILaneUnitShootHandler
    {
        private const int MaxActiveOrbiters = 5;

        private readonly ILaneModel _laneModel;
        private readonly IUnitSlotModel _unitSlotModel;
        private readonly IPixelGridModel _pixelGridModel;
        private readonly IPixelGridView _pixelGridView;
        private readonly IProjectileFactory _projectileFactory;
        private readonly ILaneUnitFactory _laneUnitFactory;
        private readonly IPixelCellFactory _pixelCellFactory;

        private readonly List<BaseLaneUnitObject> _activeOrbiters = new(MaxActiveOrbiters);

        private LaneUnitOrbitPath _orbitPath;
        private bool _isInitialized, _isDisposing;

        public LaneUnitShootHandler(ILaneModel laneModel, IUnitSlotModel unitSlotModel, IPixelGridModel pixelGridModel, IPixelGridView pixelGridView, IProjectileFactory projectileFactory, ILaneUnitFactory laneUnitFactory, IPixelCellFactory pixelCellFactory)
        {
            _laneModel = laneModel;
            _unitSlotModel = unitSlotModel;
            _pixelGridModel = pixelGridModel;
            _pixelGridView = pixelGridView;
            _projectileFactory = projectileFactory;
            _laneUnitFactory = laneUnitFactory;
            _pixelCellFactory = pixelCellFactory;

            _pixelGridView.OnViewInitialized += OnPixelGridReady;
        }

        public void Dispose()
        {
            _isDisposing = true;
            _pixelGridView.OnViewInitialized -= OnPixelGridReady;

            _activeOrbiters.Clear();
        }

        private void OnPixelGridReady()
        {
            _orbitPath = LaneUnitOrbitPathGenerator.Generate(_pixelGridView);
            _isInitialized = true;
        }

        public void OnFrontUnitTapped(BaseLaneUnitObject unit)
        {
            if (!_laneModel.TryGetLaneIndexOf(unit, out int laneIndex)) return;
            if (!CanStartOrbit(unit)) return;

            _laneModel.RemoveFrontUnit(laneIndex);
            LaunchAndRunOrbitAsync(unit).Forget();
        }

        public void OnSlotUnitTapped(int slotIndex, BaseLaneUnitObject unit)
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

            await unit.MotionAnimation.JumpToLaunchPosition(launchPos);

            bool aimLocked = ShouldStartWithInwardAim(unit);

            unit.OrbitAnimation.SetPathRotationImmediate(_orbitPath.Nodes[startIdx].PathYaw);
            if (aimLocked) unit.OrbitAnimation.SetAimImmediate();
            else unit.OrbitAnimation.ResetAimImmediate();

            await OrbitRunner.Run(unit, _orbitPath, startIdx, unit.OrbitAnimation.OrbitSpeed, aimLocked, HandleTriggerAtNode);

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
            if (_isDisposing || !unit || !pixel) return;

            var ball = _projectileFactory.GetProjectile<BallProjectileObject>();
            var spawnPos = unit.ProjectileSpawnPoint.position;
            var targetPos = pixel.transform.position;

            ball.transform.position = spawnPos;

            await ball.MoveAnimation.MoveToTarget(spawnPos, targetPos);

            if (!_isDisposing && ball)
                _projectileFactory.ReleaseProjectile(ball);

            if (_isDisposing || !pixel || !pixel.gameObject.activeInHierarchy) return;

            await pixel.CellAnimation.PlayBounceAndScaleDown();

            if (_isDisposing || !pixel || !pixel.gameObject.activeInHierarchy) return;

            _pixelCellFactory.ReleasePixelCell(pixel);
        }

        private async UniTask ReleaseUnit(BaseLaneUnitObject unit)
        {
            if (_isDisposing || !unit) return;

            _activeOrbiters.Remove(unit);

            await unit.MotionAnimation.ScaleDown();

            if (_isDisposing || !unit || !unit.gameObject.activeInHierarchy) return;

            _laneUnitFactory.ReleaseLaneUnit(unit);

            if (unit)
                unit.MotionAnimation.ResetScale();
        }

        private bool ShouldStartWithInwardAim(BaseLaneUnitObject unit)
        {
            var startNode = _orbitPath.Nodes[_orbitPath.LaunchNodeIndex];
            return OrbitLineSearcher.TryFindMatchingPixelOnLine(_pixelGridModel, startNode, unit.ColorId, out _);
        }
    }
}