using Blish_HUD;
using Glide;
using System;
using System.Reflection;

namespace Flyga.AdditionalAchievements
{
    public static class TweenExtensions
    {
        public class NullRemover : Glide.Tween.TweenerImpl
        {
            private readonly static NullRemover _sharedInstance = new NullRemover();

            public static NullRemover SharedInstance
            {
                get
                {
                    // to call AddAndRemove
                    // which will remove the unused Tweens from the toRemove queue
                    _sharedInstance.Update(0); 
                    return _sharedInstance;
                }
            }

            public NullRemover() : base() { }
        }

        private static Logger Logger = Logger.GetLogger(typeof(TweenExtensions));

        public static Tween GetIsolatedTween(this Tweener tweener, object target, object values, float duration, float delay)
        {
            object[] args = { target, duration, delay, NullRemover.SharedInstance };

            object instance = null;
            try
            {
                instance = Activator.CreateInstance(typeof(Tween), BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.CreateInstance, null, args, null, null);
            }
            catch (Exception ex)
            {
                Logger.Warn($"Unable to create isolated tween. {ex}");
            }

            if (!(instance is Tween tween))
            {
                return null;
            }

            if (values == null) // valid in case of manual timer
                return tween;

            var props = values.GetType().GetProperties();
            for (int i = 0; i < props.Length; ++i)
            {

                var property = props[i];
                var info = CreateMemberAccessor(target, property.Name, true, out Type memberType);
                var to = CreateMemberAccessor(values, property.Name, false, out _);

                var lerper = CreateLerper(tweener, memberType);

                AddLerp(tween, lerper, info, GetValue(info, target), GetValue(to, values));
            }

            return tween;
        }

        private static object CreateMemberAccessor(object target, string name, bool writeRequired, out Type propertyOrFieldType)
        {
            object[] args = { target, name, writeRequired };

            propertyOrFieldType = null;
            object instance = null;

            try
            {
                PropertyInfo propInfo = null;
                FieldInfo fieldInfo = null;
                BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

                if ((propInfo = target.GetType().GetProperty(name, flags)) != null)
                {
                    propertyOrFieldType = propInfo.PropertyType;
                }
                if ((fieldInfo = target.GetType().GetField(name, flags)) != null)
                {
                    propertyOrFieldType = fieldInfo.FieldType;
                }

                Type memberAccessorType = typeof(Tween).Assembly.GetType("Glide.MemberAccessor");
                instance = Activator.CreateInstance(memberAccessorType, BindingFlags.Instance | BindingFlags.Public | BindingFlags.CreateInstance, null, args, null, null);
            }
            catch (Exception ex)
            {
                Logger.Warn($"Unable to create MemberAccessor. {ex}");
            }

            return instance;
        }

        private static MemberLerper CreateLerper(Tweener tweener, Type propertyType)
        {
            object instance = null;
            try
            {
                Type tweenerType = tweener.GetType();

                MethodInfo methodInfo = tweenerType.BaseType.GetMethod("CreateLerper", BindingFlags.NonPublic | BindingFlags.Instance);

                object result = methodInfo.Invoke(tweener, new object[] { propertyType });

                instance = result;
            }
            catch (Exception ex)
            {
                Logger.Warn($"Unable to create lerper. {ex}");
            }

            return instance as MemberLerper;
        }

        private static void AddLerp(Tween tween, MemberLerper lerper, object info, object from, object to)
        {
            try
            {
                tween.GetType().GetMethod("AddLerp", BindingFlags.NonPublic | BindingFlags.Instance)
                    .Invoke(tween, new object[] { lerper, info, from, to });
            }
            catch (Exception ex)
            {
                Logger.Warn($"Unable to add lerp. {ex}");
            }
        }

        private static object GetValue(object memberAccessor, object target)
        {
            object value = null;
            try
            {
                value = memberAccessor.GetType().GetMethod("GetValue", BindingFlags.Public | BindingFlags.Instance)
                    .Invoke(memberAccessor, new object[] { target });
            }
            catch (Exception ex)
            {
                Logger.Warn($"Unable to get value. {ex}");
            }

            return value;
        }

        public static void UpdateManually(this Tween tween, float elapsedSeconds)
        {
            try
            {
                MethodInfo update = tween.GetType().GetMethod("Update", BindingFlags.NonPublic | BindingFlags.Instance);
                update.Invoke(tween, new object[] { elapsedSeconds });
            }
            catch (Exception ex)
            {
                Logger.Warn($"Unable to update tween manually. {ex}");
            }
        }
    }
}
