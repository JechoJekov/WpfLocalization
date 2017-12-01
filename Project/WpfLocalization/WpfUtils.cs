using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;

namespace WpfLocalization
{
    /// <summary>
    /// Provides helper methods to work with WPF objects.
    /// </summary>
    static class WpfUtils
    {
        /// <summary>
        /// Throws <see cref="InvalidOperationException"/> if the current thread is not a UI thread.
        /// </summary>
        public static void VerifyIsUIThread()
        {
            if (Dispatcher.FromThread(Thread.CurrentThread) == null)
            {
                throw new InvalidOperationException("This event can be accessed only from a UI thread.");
            }
        }

        /// <summary>
        /// Finds the root ancestor the specified <see cref="DependencyObject"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns>
        /// The parent or <c>null</c> if the a parent of type <typeparamref name="T"/> was not found.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static DependencyObject FindRootAncestor(DependencyObject obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            DependencyObject parent;

            for (
                parent = VisualTreeHelper.GetParent(obj);
                parent != null;
                parent = VisualTreeHelper.GetParent(parent)
                ) ;

            return parent;
        }

        /// <summary>
        /// Finds the first an ancestor of type <typeparamref name="T"/> of the specified <see cref="DependencyObject"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns>
        /// The parent or <c>null</c> if the a parent of type <typeparamref name="T"/> was not found.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static T FindAncestor<T>(DependencyObject obj) where T : DependencyObject
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            DependencyObject parent;

            for (
                parent = VisualTreeHelper.GetParent(obj);
                parent != null && false == parent is T;
                parent = VisualTreeHelper.GetParent(parent)
                ) ;

            return parent as T;
        }
    }
}
