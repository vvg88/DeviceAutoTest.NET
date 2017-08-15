using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeuroSoft.Prototype.Interface;

namespace NeuroSoft.DeviceAutoTest
{
    /// <summary>
    ///Модель журнала для переопределения метода удаления обследований
    /// </summary>
    public class DATJournalModel : Prototype.Interface.Database.JournalFormModel
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="manager"></param>
        public DATJournalModel(Prototype.Database.DatabaseManager manager)
            : base(manager)
        {            
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public override Prototype.Interface.Database.JournalExitCode ShowJournalForm(Prototype.Interface.Database.JournalFormModel model)
        {
            try
            {
                return base.ShowJournalForm(model);
            }
            finally
            {
                MainModel.OnCurrentConnectionChanged();
            }
        }       
    }
}
