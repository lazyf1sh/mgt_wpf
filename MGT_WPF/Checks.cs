using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MGT_WPF
{
    public static class Checks
    {
        static public void checkPcName()
        {
            string machineName = Environment.MachineName;
            machineName = machineName.ToLower();
            char[] machineNameSplitted = machineName.ToCharArray();

            string machineNameToCheck = null;
            for (int i = 0; i < 6; i++)
            {
                machineNameToCheck += machineNameSplitted[i];
            }

            if (!(machineNameToCheck == "v1user" || machineNameToCheck == "n1user"))
            {
                Environment.Exit(0);
            }
        }
        static public void checkLicence()
        {
            int currentUnixTime = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
            int licenceExpireTime = 1378190609; //03.09.2013 10:43

            if ((currentUnixTime) > (licenceExpireTime + (54 * 604800))) //множимое - неделя, прибавляем по 4 к множителю - это ещё месяц
            {
                MessageBox.Show("Пора обновиться : )");
                Environment.Exit(0);
            }
        }
        static public void checkLocation()
        {
            string programPath = AppDomain.CurrentDomain.BaseDirectory;
            string[] programPathSplitted = programPath.Split(new Char[] { ':' });
            if ((programPathSplitted[0].ToLower() != "c") && (programPathSplitted[0].ToLower() != "d") && (programPathSplitted[0].ToLower() != "p") && (programPathSplitted[0].ToLower() != "e"))
            {
                MessageBox.Show("Сначала скопируйте на локальный диск.");
                Environment.Exit(0);
            }

        }
    }
}
