using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TaleWorlds.CampaignSystem;

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
    }
}
