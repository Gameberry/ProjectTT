using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameBerry
{
    public static class InGameUtil
    {
        //public static double ElementCountDamageRatio(V2Enum_ElementType myelement, V2Enum_ElementType damageelement)
        //{
        //    if (myelement == V2Enum_ElementType.None || damageelement == V2Enum_ElementType.None)
        //        return 1.0;

        //    double defaultvalue = 1.0;

        //    if (myelement == V2Enum_ElementType.Light)
        //    {
        //        if (damageelement == V2Enum_ElementType.Darkness)
        //            return defaultvalue + Define.Skill_Element_Damage;
        //    }
        //    else if (myelement == V2Enum_ElementType.Darkness)
        //    {
        //        if (damageelement == V2Enum_ElementType.Light)
        //            return defaultvalue + Define.Skill_Element_Damage;
        //    }
        //    if (myelement == V2Enum_ElementType.Water)
        //    {
        //        if (damageelement == V2Enum_ElementType.Grass)
        //            return defaultvalue + Define.Skill_Element_Damage;
        //        else if (damageelement == V2Enum_ElementType.Fire)
        //            return defaultvalue - Define.Skill_Element_Damage;
        //    }
        //    if (myelement == V2Enum_ElementType.Fire)
        //    {
        //        if (damageelement == V2Enum_ElementType.Water)
        //            return defaultvalue + Define.Skill_Element_Damage;
        //        else if (damageelement == V2Enum_ElementType.Grass)
        //            return defaultvalue - Define.Skill_Element_Damage;
        //    }
        //    if (myelement == V2Enum_ElementType.Grass)
        //    {
        //        if (damageelement == V2Enum_ElementType.Fire)
        //            return defaultvalue + Define.Skill_Element_Damage;
        //        else if (damageelement == V2Enum_ElementType.Water)
        //            return defaultvalue - Define.Skill_Element_Damage;
        //    }

        //    return defaultvalue;
        //}

        public static double ElementCountDamageRatio(V2Enum_ElementType myelement, V2Enum_ElementType damageelement)
        {
            //if (myelement == V2Enum_ElementType.None)
            //    return Define.ElementDamageFactorBase;

            //double defaultvalue = Define.ElementDamageFactorBase;

            //if (myelement == V2Enum_ElementType.Light)
            //{
            //    if (damageelement == V2Enum_ElementType.Darkness)
            //        return Define.ElementDamageFactorBenefit;
            //}
            //else if (myelement == V2Enum_ElementType.Darkness)
            //{
            //    if (damageelement == V2Enum_ElementType.Light)
            //        return Define.ElementDamageFactorBenefit;
            //}
            //else if (myelement == V2Enum_ElementType.Water)
            //{
            //    if (damageelement == V2Enum_ElementType.Grass)
            //        return Define.ElementDamageFactorBenefit;
            //    else if (damageelement == V2Enum_ElementType.Fire)
            //        return Define.ElementDamageFactorPenalty;
            //}
            //else if (myelement == V2Enum_ElementType.Fire)
            //{
            //    if (damageelement == V2Enum_ElementType.Water)
            //        return Define.ElementDamageFactorBenefit;
            //    else if (damageelement == V2Enum_ElementType.Grass)
            //        return Define.ElementDamageFactorPenalty;
            //}
            //else if (myelement == V2Enum_ElementType.Grass)
            //{
            //    if (damageelement == V2Enum_ElementType.Fire)
            //        return Define.ElementDamageFactorBenefit;
            //    else if (damageelement == V2Enum_ElementType.Water)
            //        return Define.ElementDamageFactorPenalty;
            //}

            //return defaultvalue;

            return 1.0;
        }

        public static bool IsCounterElement(V2Enum_ElementType myelement, V2Enum_ElementType damageelement)
        {
            if (myelement == V2Enum_ElementType.None || damageelement == V2Enum_ElementType.None)
                return false;

            if (myelement == V2Enum_ElementType.Light)
            {
                if (damageelement == V2Enum_ElementType.Darkness)
                    return true;
            }
            else if (myelement == V2Enum_ElementType.Darkness)
            {
                if (damageelement == V2Enum_ElementType.Light)
                    return true;
            }
            if (myelement == V2Enum_ElementType.Water)
            {
                if (damageelement == V2Enum_ElementType.Grass)
                    return true;
            }
            if (myelement == V2Enum_ElementType.Fire)
            {
                if (damageelement == V2Enum_ElementType.Water)
                    return true;
            }
            if (myelement == V2Enum_ElementType.Grass)
            {
                if (damageelement == V2Enum_ElementType.Fire)
                    return true;
            }

            return false;
        }
    }
}