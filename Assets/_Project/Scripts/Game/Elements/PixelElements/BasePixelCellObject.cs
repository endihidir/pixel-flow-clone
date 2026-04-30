using Core.Pool.Services;
using Game.Level.Data;
using NaughtyAttributes;
using UnityEngine;

namespace Game.Grid.Item
{
    public abstract class BasePixelCellObject : PooledObject
    {
        [field: SerializeField, ReadOnly] public Vector2Int Coord { get; private set; }
        [field: SerializeField, ReadOnly] public ColorId ColorId { get; private set; }
        [field: SerializeField] public Transform PixelRootTransform { get; private set; }
        [field: SerializeField] public MeshRenderer PixelRenderer { get; private set; }
        
        private MaterialPropertyBlock _materialPropertyBlock;
        
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        private void Awake()
        {
            _materialPropertyBlock = new MaterialPropertyBlock();
        }

        public void Initialize(ColorId colorId, Color color)
        {
            ColorId = colorId;
            UpdateIdentity();
            ApplyColor(color);
        }

        public void SetCoord(Vector2Int coord) => Coord = coord;
        public void SetPosition(Vector3 position) => transform.position = position;
        public void SetParent(Transform parent) => transform.SetParent(parent);
        
        public void SetScale(Vector3 scale) => PixelRootTransform.localScale = scale;

        private void ApplyColor(Color color)
        {
            _materialPropertyBlock ??= new MaterialPropertyBlock();
            PixelRenderer.GetPropertyBlock(_materialPropertyBlock);
            _materialPropertyBlock.SetColor(BaseColorId, color);
            PixelRenderer.SetPropertyBlock(_materialPropertyBlock);
        }

        protected override void OnDeactivate() => ResetItem();
        
        protected void UpdateIdentity()
        {
#if UNITY_EDITOR
            name = ToString();
#endif
        }

        public void ResetItem()
        {
            ColorId = ColorId.None;
            Coord = new Vector2Int(-1, -1);
            SetPosition(Vector3.zero);
        }
        
        public override string ToString() => $"Id: {ColorId}";
    }
}