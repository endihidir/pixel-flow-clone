using System;
using Game.Models;
using Game.Views;
using UnityEngine;

namespace Game.Presenters
{
    public sealed class PixelGridPresenter : IDisposable
    {
        private readonly IPixelGridModel _model;
        private readonly IPixelGridView _view;

        public PixelGridPresenter(IPixelGridModel model, IPixelGridView view)
        {
            _model = model;
            _view = view;
            
            _view.OnViewInitialized += Initialize;
        }

        private void Initialize()
        {
            for (var y = 0; y < _model.Height; y++)
            {
                for (var x = 0; x < _model.Width; x++)
                {
                    var coord = new Vector2Int(x, y);
                    var pixelCell = _model.GetGridObject(coord);
                    if(!pixelCell) continue;
                    _view.PlacePixel(pixelCell, coord);
                }
            }
        }

        public void Dispose()
        {
            _view.OnViewInitialized -= Initialize;
        }
    }
}
