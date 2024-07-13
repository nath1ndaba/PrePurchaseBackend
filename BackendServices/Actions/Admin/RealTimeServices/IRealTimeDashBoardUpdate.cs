using BackendServices.Models.RealTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackendServices.Actions.Admin.RealTimeServices
{
    public interface IRealTimeDashBoardUpdate
    {
        void OnClockingAction(string companyId);
        Task<DashboardUIData> GetDashboardUIData(string companyId);
    }
}
