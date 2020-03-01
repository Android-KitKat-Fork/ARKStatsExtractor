﻿using ARKBreedingStats.species;
using ARKBreedingStats.values;
using System;

namespace ARKBreedingStats
{
    public static class StatValueCalculation
    {
        //private const double ROUND_UP_DELTA = 0.0001; // remove for now. Rounding issues should be handled during extractino with value-ranges.

        public static double CalculateValue(Species species, int stat, int levelWild, int levelDom, bool dom, double tamingEff, double imprintingBonus, bool roundToIngamePrecision = true)
        {
            if (species == null)
                return 0;

            // if stat is generally available but level is set to -1 (== unknown), return -1 (== unknown)
            if (levelWild < 0 && species.stats[stat].IncPerWildLevel != 0)
                return -1;

            double add = 0, domMult = 1, imprintingM = 1, tamedBaseHP = 1;
            if (dom)
            {
                add = species.stats[stat].AddWhenTamed;
                double domMultAffinity = species.stats[stat].MultAffinity;
                // the multiplicative bonus is only multiplied with the TE if it is positive (i.e. negative boni won't get less bad if the TE is low)
                if (domMultAffinity >= 0)
                    domMultAffinity *= tamingEff;
                domMult = (tamingEff >= 0 ? (1 + domMultAffinity) : 1) * (1 + levelDom * species.stats[stat].IncPerTamedLevel);
                if (imprintingBonus > 0
                    && species.statImprintMult[stat] != 0
                    )
                    imprintingM = 1 + species.statImprintMult[stat] * imprintingBonus * Values.V.currentServerMultipliers.BabyImprintingStatScaleMultiplier;
                if (stat == 0)
                    tamedBaseHP = (float)species.TamedBaseHealthMultiplier;
            }
            //double result = Math.Round((species.stats[stat].BaseValue * tamedBaseHP * (1 + species.stats[stat].IncPerWildLevel * levelWild) * imprintingM + add) * domMult, Utils.precision(stat), MidpointRounding.AwayFromZero);
            // double is too precise and results in wrong values due to rounding. float results in better values, probably ARK uses float as well.
            // or rounding first to a precision of 7, then use the rounding of the precision
            //double resultt = Math.Round((species.stats[stat].BaseValue * tamedBaseHP * (1 + species.stats[stat].IncPerWildLevel * levelWild) * imprintingM + add) * domMult, 7);
            //resultt = Math.Round(resultt, Utils.precision(stat), MidpointRounding.AwayFromZero);

            // adding an epsilon to handle rounding-errors
            double result = (species.stats[stat].BaseValue * tamedBaseHP *
                    (1 + species.stats[stat].IncPerWildLevel * levelWild) * imprintingM + add) *
                    domMult;// + (Utils.precision(stat) == 3 ? ROUND_UP_DELTA * 0.01 : ROUND_UP_DELTA);

            if (result <= 0) return 0;

            if (roundToIngamePrecision)
                return Math.Round(result, Utils.precision(stat), MidpointRounding.AwayFromZero);

            return result;
        }
    }
}
