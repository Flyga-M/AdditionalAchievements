using Blish_HUD.Graphics.UI;
using System;

namespace Flyga.AdditionalAchievements.UI.Models
{
    public class ViewCreationData<TCreationParameter> : ViewCreationData
    {
        private readonly Func<TCreationParameter, IView> _viewCreatorGeneric;

        public ViewCreationData(Func<TCreationParameter, IView> viewCreator, TCreationParameter creationParameter) : base()
        {
            _viewCreatorGeneric = viewCreator;

            _viewCreator = GetViewFromObject;
            _creationParameter = creationParameter;
        }

        private IView GetViewFromObject(object param)
        {
            if (param == null)
            {
                return _viewCreatorGeneric(default);
            }
            
            if (!(typeof(TCreationParameter).IsAssignableFrom(param.GetType())))
            {
                throw new InvalidOperationException($"Unable to get view from object {param}, because it is not of the expected type " +
                    $"{typeof(TCreationParameter)}.");
            }

            return _viewCreatorGeneric((TCreationParameter)param);
        }

        public ViewCreationData(Func<IView> viewCreator) : base (viewCreator)
        {
            _viewCreatorGeneric = (TCreationParameter _) => viewCreator(); // not strictly neccessary atm
        }
    }
    
    public class ViewCreationData
    {
        protected Func<object, IView> _viewCreator;

        protected object _creationParameter;

        protected ViewCreationData() { }

        public ViewCreationData(Func<object, IView> viewCreator, object creationParameter)
        {
            _viewCreator = viewCreator;
            _creationParameter = creationParameter;
        }

        public ViewCreationData(Func<IView> viewCreator)
        {
            _viewCreator = (object _) => viewCreator();
            _creationParameter = null;
        }

        public IView GetView()
        {
            return _viewCreator(_creationParameter);
        }
    }
}
