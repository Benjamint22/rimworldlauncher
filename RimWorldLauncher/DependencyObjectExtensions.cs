using System.Windows;
using System.Windows.Media;

namespace RimWorldLauncher
{
    public static class DependencyObjectExtensions
    {
        /// <summary>
        ///     Obtains the parent element of <paramref name="current" />.
        /// </summary>
        /// <param name="current">The element to obtain the parent of.</param>
        /// <returns>The parent element or null.</returns>
        public static DependencyObject GetParent(this DependencyObject current)
        {
            return VisualTreeHelper.GetParent(current);
        }

        /// <summary>
        ///     Obtains the first ancestor of <paramref name="current" /> with the type <typeparamref name="T" /> or null.
        /// </summary>
        /// <typeparam name="T">The type to look for.</typeparam>
        /// <param name="current">The element to obtain the ancestor of.</param>
        /// <returns>An ancestor of <paramref name="current" /> with the type <typeparamref name="T" /> or null.</returns>
        public static T FindAncestor<T>(this DependencyObject current)
            where T : DependencyObject
        {
            if (current is T) return current as T;
            var parent = current.GetParent();
            if (parent == null) return null;
            return parent.FindAncestor<T>();
        }
    }
}