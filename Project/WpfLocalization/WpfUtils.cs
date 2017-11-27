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

#if DEPRECATED

        /// <summary>
        /// Creates a copy of the specified binding.
        /// </summary>
        /// <param name="binding"></param>
        /// <returns></returns>
        public static BindingBase CloneBinding(BindingBase binding)
        {
            if (binding == null)
            {
                throw new ArgumentNullException(nameof(binding));
            }

            if (binding is Binding regularBinding)
            {
                var result = new Binding
                {
                    AsyncState = regularBinding.AsyncState,
                    BindingGroupName = regularBinding.BindingGroupName,
                    BindsDirectlyToSource = regularBinding.BindsDirectlyToSource,
                    Converter = regularBinding.Converter,
                    ConverterCulture = regularBinding.ConverterCulture,
                    ConverterParameter = regularBinding.ConverterParameter,
                    FallbackValue = regularBinding.FallbackValue,
                    IsAsync = regularBinding.IsAsync,
                    Mode = regularBinding.Mode,
                    NotifyOnSourceUpdated = regularBinding.NotifyOnSourceUpdated,
                    NotifyOnTargetUpdated = regularBinding.NotifyOnTargetUpdated,
                    NotifyOnValidationError = regularBinding.NotifyOnValidationError,
                    Path = regularBinding.Path,
                    StringFormat = regularBinding.StringFormat,
                    TargetNullValue = regularBinding.TargetNullValue,
                    UpdateSourceExceptionFilter = regularBinding.UpdateSourceExceptionFilter,
                    UpdateSourceTrigger = regularBinding.UpdateSourceTrigger,
                    ValidatesOnDataErrors = regularBinding.ValidatesOnDataErrors,
                    ValidatesOnExceptions = regularBinding.ValidatesOnExceptions,
                    XPath = regularBinding.XPath,
                };

                if (regularBinding.Source != null)
                {
                    result.Source = regularBinding.Source;
                }
                if (regularBinding.ElementName != null)
                {
                    result.ElementName = regularBinding.ElementName;
                }
                if (regularBinding.RelativeSource != null)
                {
                    result.RelativeSource = regularBinding.RelativeSource;
                }

                foreach (var validationRule in regularBinding.ValidationRules)
                {
                    result.ValidationRules.Add(validationRule);
                }

                return result;
            }
            else if (binding is MultiBinding multiBinding)
            {
                var result = new MultiBinding
                {
                    BindingGroupName = multiBinding.BindingGroupName,
                    Converter = multiBinding.Converter,
                    ConverterCulture = multiBinding.ConverterCulture,
                    ConverterParameter = multiBinding.ConverterParameter,
                    FallbackValue = multiBinding.FallbackValue,
                    Mode = multiBinding.Mode,
                    NotifyOnSourceUpdated = multiBinding.NotifyOnSourceUpdated,
                    NotifyOnTargetUpdated = multiBinding.NotifyOnTargetUpdated,
                    NotifyOnValidationError = multiBinding.NotifyOnValidationError,
                    StringFormat = multiBinding.StringFormat,
                    TargetNullValue = multiBinding.TargetNullValue,
                    UpdateSourceExceptionFilter = multiBinding.UpdateSourceExceptionFilter,
                    UpdateSourceTrigger = multiBinding.UpdateSourceTrigger,
                    ValidatesOnDataErrors = multiBinding.ValidatesOnDataErrors,
                    ValidatesOnExceptions = multiBinding.ValidatesOnDataErrors,
                };

                foreach (var validationRule in multiBinding.ValidationRules)
                {
                    result.ValidationRules.Add(validationRule);
                }

                foreach (var item in multiBinding.Bindings)
                {
                    result.Bindings.Add(CloneBinding(item));
                }

                return result;
            }
            else if (binding is PriorityBinding priorityBinding)
            {
                var result = new PriorityBinding()
                {
                    BindingGroupName = priorityBinding.BindingGroupName,
                    FallbackValue = priorityBinding.FallbackValue,
                    StringFormat = priorityBinding.StringFormat,
                    TargetNullValue = priorityBinding.TargetNullValue,
                };

                foreach (var item in priorityBinding.Bindings)
                {
                    result.Bindings.Add(CloneBinding(item));
                }

                return result;
            }
            else
            {
                throw new NotSupportedException($"Unsupported binding type '{binding.GetType().FullName}'.");
            }
        }

#endif
    }
}
