using Blish_HUD.Content;
using Blish_HUD.Controls;
using System;

namespace Flyga.AdditionalAchievements.UI.Controls
{
    /// <summary>
    /// A <see cref="MenuItem"/> that holds additional data of the given type <see cref="TData"/>.
    /// </summary>
    public class MenuItemWithData<TData> : MenuItemWithData
    {
        /// <summary>
        /// <inheritdoc cref="MenuItemWithData.Data"/>
        /// </summary>
        /// <remarks>
        /// <see cref="MenuItemWithData.DataType"/> is <see cref="TData"/>. 
        /// Will return the default value (may be <see langword="null"/>), if <see cref="Data"/> has not been set.
        /// </remarks>
        public new TData Data
        {
            get
            {
                if (base.Data == null)
                {
                    return default;
                }
                
                return (TData)base.Data;
            }
            set
            {
                base.Data = value;
            }
        }

        public MenuItemWithData() : base()
        {
            DataType = typeof(TData);
        }

        public MenuItemWithData(string text) : base(text)
        {
            DataType = typeof(TData);
        }

        public MenuItemWithData(string text, AsyncTexture2D icon) : base(text, icon)
        {
            DataType = typeof(TData);
        }
    }

    /// <summary>
    /// A <see cref="MenuItem"/> that holds additional data.
    /// </summary>
    public class MenuItemWithData : MenuItem
    {
        private object _data;
        
        /// <summary>
        /// The <see cref="Type"/> of the <see cref="Data"/>.
        /// </summary>
        /// <remarks>
        /// Will only hold something other than <see cref="object"/>, if the <see cref="MenuItemWithData"/> is of the generic type 
        /// <see cref="MenuItemWithData{TData}"/>.
        /// </remarks>
        public Type DataType { get; protected set; } = typeof(object);

        /// <summary>
        /// The data, that the <see cref="MenuItemWithData"/> is holding. Must be of the <see cref="DataType"/> or a subclass.
        /// </summary>
        /// <remarks>
        /// May be <see langword="null"/>, even if the <see cref="DataType"/> is not nullable.
        /// </remarks>
        public object Data
        {
            get
            {
                return _data;
            }
            set
            {
                if (value == null)
                {
                    _data = null;
                    return;
                }
                
                if (!DataType.IsAssignableFrom(value.GetType()))
                {
                    throw new InvalidOperationException($"Unable to set {nameof(Data)} for {typeof(MenuItemWithData)} to given value {value}. " +
                        $"{nameof(DataType)} ({DataType}) is not assignable from the value type {value.GetType()}.");
                }

                _data = value;
            }
        }

        public MenuItemWithData() : base() { }

        public MenuItemWithData(string text) : base(text) { }

        public MenuItemWithData(string text, AsyncTexture2D icon) : base(text, icon) { }
    }
}
