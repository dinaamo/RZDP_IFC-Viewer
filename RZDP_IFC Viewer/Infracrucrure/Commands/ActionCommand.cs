using IFC_Table_View.Infracrucrure.Commands.Base;

namespace IFC_Table_View.Infracrucrure.Commands
{
    internal class ActionCommand : BaseCommand
    {
        private readonly Action<object> _Execute;
        private readonly Func<object, bool> _CanExecute;

        public ActionCommand(Action<object> Execute, Func<object, bool> CanExecute = null)
        {
            _Execute = Execute ?? throw new ArgumentException(nameof(Execute));
            _CanExecute = CanExecute;
        }

        public override bool CanExecute(object parameter)
        {
            if (_CanExecute(parameter) == null)
            {
                return true;
            }
            else
            {
                return _CanExecute(parameter);
            }
        }

        public override void Execute(object parameter) => _Execute(parameter);
    }
}