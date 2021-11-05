using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyCalendar.Controller;

namespace MyCalendar.Domain
{
    enum AlertType{
        ERROR_ALREADY_EXISTS,
        ERROR_FILE_NOT_EXISTS
    }

    class AlertManager
    {
        private static AlertManager instance;

        public static AlertManager getInstance()
        {
            if(instance == null)
            {
                instance = new AlertManager();
            }

            return instance;
        }

        public void ShowAlert(AlertType alertType, object[] args)
        {
            switch(alertType)
            {
                case AlertType.ERROR_ALREADY_EXISTS:
                    ShowAlreadyExists(args);
                    break;
            }
        }

        private void ShowAlreadyExists(object[] args)
        {
            Console.WriteLine($"[{args[0]}. {args[1]}] is already exists.");
        }

    }
}
