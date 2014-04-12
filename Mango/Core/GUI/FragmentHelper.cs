using System;
using System.Windows;
using System.Reflection;

namespace Mango.Core.GUI
{
    public class FragmentHelper
    {
        public static UIElement ExtractUI<WindowClass>() where WindowClass : Window
        {
            return ExtractUI<WindowClass, UIElement>();
        }

        public static ReturnType ExtractUI<WindowClass, ReturnType>() where WindowClass : Window where ReturnType : UIElement
        {
            try
            {
                Type @class = typeof(WindowClass);
                Window window = (Window)Activator.CreateInstance(@class);

                return ExtractUI<ReturnType>(window);
            }
            catch
            {
                return null;
            }
        }

        public static UIElement ExtractUI(Window from)
        {
            return ExtractUI<UIElement>(from);
        }

        public static ReturnType ExtractUI<ReturnType>(Window from)
            where ReturnType : UIElement
        {
            try
            {
                object obj = from.Content;
                if (obj == null || !(obj is ReturnType)) return null;
                ReturnType element = (ReturnType)obj;

                MethodInfo removeChild = from.GetType().GetMethod("RemoveLogicalChild", BindingFlags.NonPublic | BindingFlags.Instance);
                removeChild.Invoke(from, new object[] { element });

                from.Close();
                return element;
            }
            catch
            {
                return null;
            }
        }
    }
}
