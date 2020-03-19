using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using Magicallity.Client.Helpers;
using Magicallity.Shared;
using Magicallity.Shared.Attributes;
using Magicallity.Shared.Enums;
using Magicallity.Shared.Helpers;

namespace Magicallity.Client.Jobs.Criminal
{
    public class GunShotAlerts : JobClass
    {
        private Random rand;
        private bool checkingAlertStatus = false;
        private string previousFireLocation;
        private int timesAlerted = 0;

        public GunShotAlerts()
        {
            rand = new Random();
        }

        [DynamicTick(TickUsage.Shooting)]
        private async Task onShooting()
        {
            if (rand.NextBool() && !checkingAlertStatus) // 50% chance
            {
                checkingAlertStatus = true;

                var closePed = GTAHelpers.GetCloestPed(512.0f, customFindFunc: o => !o.IsPlayer && !o.IsDead && o.IsHuman);
                if (closePed == null) return;
                Log.Verbose("Found a close ped sending alert");

                var gender = CitizenFX.Core.Native.API.IsPedMale(Cache.PlayerPed.Handle) ? "Male" : "Female";
                var currentLocation = GTAHelpers.GetLocationString();

                if (previousFireLocation == currentLocation)
                {
                    timesAlerted += 1;
                }

                previousFireLocation = currentLocation;

                if (timesAlerted >= 4) return;

                Client.Instance.TriggerServerEvent("Alerts.SendCADAlert", $"10-32", $"{gender} firing a weapon", currentLocation);

                await BaseScript.Delay(rand.Next(2000, 10000));
                checkingAlertStatus = false;
            }
        }

        [DynamicTick]
        private async Task ResetAlertTick()
        {
            if (timesAlerted >= 4)
            {
                await BaseScript.Delay(60000); // 1 minute
                timesAlerted = 0;
            }
        }
    }
}
