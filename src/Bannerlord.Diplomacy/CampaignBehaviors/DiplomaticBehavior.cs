using Diplomacy.Models;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;

namespace Diplomacy.CampaignBehaviors
{
    internal sealed class DiplomaticBehavior : CampaignBehaviorBase
    {
        public DiplomaticBehavior()
        {
            return;
        }
        public override void RegisterEvents()
        {
            CampaignEvents.DailyTickClanEvent.AddNonSerializedListener(this, ConsiderDiplomatic);
        }

        private void ConsiderDiplomatic(Clan clan)
        {
            //Check for:
            // 1. Is kingdom Faction
            // 2. Is the Leader clan
            // 3. Is not the human
            // 4. Is the correct day
            if (clan.MapFaction.IsKingdomFaction &&
                clan.MapFaction.Leader == clan.Leader &&
                !clan.Leader.IsHumanPlayerCharacter &&
                (int) CampaignTime.Now.ToDays % 10 == 0)
            {
                Kingdom ourselfKingdom = clan.Kingdom;
                // get number of soldiers in our kingdom
                int soldierCount = CalculateSoldierCount(ourselfKingdom);

                float reward = this.CalculateReward(ourselfKingdom);
                IEnumerable<IFaction> kingdoms = clan.Kingdom.FactionsAtWarWith;
                IEnumerable<Settlement> settlements = Settlement.All.Where(s => s.IsTown || s.IsCastle);
                return;
            }
            return;
        }

        float CalculateReward(Kingdom Kingdom)
        {
            float prosperity = Kingdom.Settlements
                .Where(s => s.IsFortification && s.Town != null)
                .Sum(s => s.Town.Prosperity);
            return prosperity;
        }

        /// <summary>
        /// Calculates total soldiers belonging to the kingdom by summing the man-count of every mobile party.
        /// Uses Kingdom.AllParties and each party's MemberRoster.TotalManCount.
        /// </summary>
        /// <param name="kingdom">The kingdom to inspect.</param>
        /// <returns>Total number of soldiers (int).</returns>
        private int CalculateSoldierCount(Kingdom kingdom)
        {
            int total = kingdom.AllParties
                .Where(s => s.IsLordParty)
                .Sum(s => s.MemberRoster.TotalManCount);

            return total;
        }

        public override void SyncData(IDataStore dataStore)
        {
            return;
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("export_settlements", "diplomacy")]
        public static string ManualExportSettlements(List<string> args)
        {
            // 1. Check if the user provided a path
            if (args == null || args.Count == 0)
            {
                return "Error: You must provide an absolute path. Usage: versailles.export_settlements \"C:\\path\\to\\file.json\"";
            }

            // 2. Join args in case the path has spaces and isn't quoted
            string fullPath = string.Join(" ", args).Replace("\"", "");

            try
            {
                // 3. Ensure the directory exists
                string directory = Path.GetDirectoryName(fullPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // 4. Gather Data
                var exportList = Settlement.All
                    .Where(s => s.IsTown || s.IsCastle)
                    .Select(s => new SettlementExportData
                    {
                        Id = s.StringId,
                        Name = s.Name.ToString(),
                        Type = s.IsTown ? "Town" : "Castle",
                        OwnerKingdom = s.OwnerClan?.Kingdom?.Name.ToString() ?? "Neutral",
                        Prosperity = s.Town?.Prosperity ?? 0f,
                        Food = s.Town?.FoodStocks ?? 0f,
                        GarrisonSize = s.Town?.GarrisonParty?.MemberRoster.TotalManCount ?? 0,
                        PosX = s.GetPosition().x,
                        PosY = s.GetPosition().y
                    }).ToList();

                // 5. Serialize and Write
                string json = JsonConvert.SerializeObject(exportList, Formatting.Indented);
                File.WriteAllText(fullPath, json);

                return $"Success! Exported {exportList.Count} settlements to: {fullPath}";
            }
            catch (Exception ex)
            {
                return $"Internal Error: {ex.Message}";
            }
        }

    }
}
