using Core.Pool.Services;
using Game.Level.Data;
using Game.Modules;
using NaughtyAttributes;
using TMPro;
using UnityEngine;

namespace Game.Lane.Item
{
    public abstract class BaseLaneUnitObject : PooledObject
    {
        [field: SerializeField, ReadOnly] public ColorId ColorId { get; protected set; }
        [field: SerializeField, ReadOnly] public int Ammo { get; protected set; }
        [field: SerializeField] public Transform ProjectileSpawnPoint { get; private set; }
        [field: SerializeField] public MeshRenderer UnitRenderer { get; private set; }
        [field: SerializeField] public TMP_Text AmmoText { get; private set; }
        [field: SerializeField] public LaneUnitAnimationModule Animation { get; private set; }

        private MaterialPropertyBlock _materialPropertyBlock;
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        private void Awake() => _materialPropertyBlock = new MaterialPropertyBlock();

        public void Initialize(ColorId colorId, int ammo, Color color)
        {
            ColorId = colorId;
            Ammo = ammo;
            ApplyColor(color);
            RefreshAmmoText();
            UpdateIdentity();
        }

        public void SetParent(Transform parent) => transform.SetParent(parent);
        public void SetLocalPosition(Vector3 localPosition) => transform.localPosition = localPosition;
        public void SetLocalRotation(Quaternion localRotation) => transform.localRotation = localRotation;

        public void ConsumeAmmo(int amount)
        {
            Ammo = Mathf.Max(0, Ammo - amount);
            RefreshAmmoText();
        }

        private void RefreshAmmoText() => AmmoText?.SetText(Ammo.ToString());

        private void ApplyColor(Color color)
        {
            _materialPropertyBlock ??= new MaterialPropertyBlock();
            UnitRenderer.GetPropertyBlock(_materialPropertyBlock);
            _materialPropertyBlock.SetColor(BaseColorId, color);
            UnitRenderer.SetPropertyBlock(_materialPropertyBlock);
        }

        protected override void OnDeactivate() => ResetUnit();

        protected void UpdateIdentity()
        {
#if UNITY_EDITOR
            name = ToString();
#endif
        }

        public void ResetUnit()
        {
            Animation?.Dispose();
            ColorId = ColorId.None;
            Ammo = 0;
            RefreshAmmoText();
            SetLocalPosition(Vector3.zero);
        }

        public override string ToString() => $"Color: {ColorId}, Ammo: {Ammo}";
    }
}