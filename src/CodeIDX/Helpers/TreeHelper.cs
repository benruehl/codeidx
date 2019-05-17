using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Collections;

namespace CodeIDX.Helpers
{
    public static class TreeHelper
    {

        public static T FindVisualAncestor<T>(DependencyObject obj, Predicate<T> predicate = null)
            where T : class
        {
            try
            {
                DependencyObject ancestor = VisualTreeHelper.GetParent(obj);
                while (ancestor != null)
                {
                    T ancestorAsT = ancestor as T;
                    if (ancestorAsT != null && (predicate == null || predicate(ancestorAsT)))
                        return ancestorAsT;

                    ancestor = VisualTreeHelper.GetParent(ancestor);
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        public static T FindLogicalAncestor<T>(DependencyObject obj, Predicate<T> predicate = null)
            where T : class
        {
            try
            {
                DependencyObject ancestor = LogicalTreeHelper.GetParent(obj);
                while (ancestor != null)
                {
                    T ancestorAsT = ancestor as T;
                    if (ancestorAsT != null && (predicate == null || predicate(ancestorAsT)))
                        return ancestorAsT;

                    ancestor = LogicalTreeHelper.GetParent(ancestor);
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Looks for the ancestor on visual route as far as possible and continues on logical route if neccesary
        /// </summary>
        public static T FindAncestor<T>(DependencyObject obj, Predicate<T> predicate = null)
            where T : class
        {
            try
            {
                DependencyObject ancestor = VisualTreeHelper.GetParent(obj);
                while (ancestor != null)
                {
                    T ancestorAsT = ancestor as T;
                    if (ancestorAsT != null && (predicate == null || predicate(ancestorAsT)))
                        return ancestorAsT;

                    ancestor = VisualTreeHelper.GetParent(ancestor) ?? LogicalTreeHelper.GetParent(ancestor);
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        public static T FindVisualChild<T>(DependencyObject obj, Predicate<T> predicate = null)
            where T : class
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is T)
                    return child as T;
                else
                {
                    T childOfChild = FindVisualChild<T>(child);
                    if (childOfChild != null && (predicate == null || predicate(childOfChild)))
                        return childOfChild;
                }
            }
            return null;
        }

        public static IEnumerable<DependencyObject> GetVisualChildren(DependencyObject obj)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
                yield return VisualTreeHelper.GetChild(obj, i);
        }

        /// <summary>
        /// Returns the route to the ancestor in order starting with the ancestor, ending with the start object.
        /// </summary>
        public static IEnumerable<DependencyObject> GetRouteToAncestor<T>(DependencyObject obj, Predicate<T> predicate = null)
            where T : class
        {
            Stack<DependencyObject> visualRoute = new Stack<DependencyObject>();
            try
            {
                visualRoute.Push(obj);
                DependencyObject ancestor = VisualTreeHelper.GetParent(obj);
                while (ancestor != null)
                {
                    visualRoute.Push(ancestor);
                    T ancestorAsT = ancestor as T;
                    if (ancestorAsT != null && (predicate == null || predicate(ancestorAsT)))
                        return visualRoute;

                    ancestor = VisualTreeHelper.GetParent(ancestor) ?? LogicalTreeHelper.GetParent(ancestor);
                }

                return Enumerable.Empty<DependencyObject>();
            }
            catch
            {
                return Enumerable.Empty<DependencyObject>();
            }
        }

    }
}
