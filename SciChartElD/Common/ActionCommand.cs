// *************************************************************************************
// SCICHART © Copyright ABT Software Services Ltd. 2011-2012. All rights reserved.
//  
// Web: http://www.scichart.com
// Support: info@abtsoftware.co.uk
//  
// ActionCommand.cs is part of SciChart Examples, a High Performance WPF & Silverlight Chart. 
// For full terms and conditions of the SciChart license, see http://www.scichart.com/scichart-eula/
//  
// SciChart Examples source code is provided free of charge on an "As-Is" basis to support
// and provide examples of how to use the SciChart component. You bear the risk of using it. 
// The authors give no express warranties, guarantees or conditions. You may have additional 
// consumer rights under your local laws which this license cannot change. To the extent 
// permitted under your local laws, the contributors exclude the implied warranties of 
// merchantability, fitness for a particular purpose and non-infringement. 
// *************************************************************************************
using System;
using System.Diagnostics;
using System.Windows.Input;

namespace SciChartElD
{
    /// <summary>
    /// Provides an ICommand derived class allowing delegates to be invokved directly on the view model 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ActionCommand<T> : ICommand where T : class
    {
        readonly Action<T> _execute;
        readonly Predicate<T> _canExecute;

        public ActionCommand(Action<T> execute)
            : this(execute, null)
        {
        }

        public ActionCommand(Action<T> execute, Predicate<T> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException("execute cannot be null");

            _execute = execute;
            _canExecute = canExecute;
        }

        [DebuggerStepThrough]
        public bool CanExecute(object parameter)
        {
            return _canExecute == null ? true : _canExecute((T)parameter);
        }

        public event EventHandler CanExecuteChanged;

        public void RaiseCanExecuteChanged()
        {
            var handler = CanExecuteChanged;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        public void Execute(object parameter)
        {
            var param = parameter as T;

            if (parameter != null && param == null)
                throw new InvalidOperationException("Wrong type of parameter being passed in.  Expected [" + typeof(T) + "]but was [" + parameter.GetType() + "]");

            if (!CanExecute(param))
                throw new InvalidOperationException("Should not try to execute command that cannot be executed");

            _execute(param);
        }
    }

    public class ActionCommand : ActionCommand<object>
    {
        public ActionCommand(Action execute)
            : base(arg => execute())
        { }

        public ActionCommand(Action execute, Func<bool> canExecute)
            : base(arg => execute(), arg => canExecute())
        { }
    }
}
