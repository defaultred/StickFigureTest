using System;
using UnityEngine;



public struct AnimationUnits
{
    public const int MaxNbAnimUnits = 6;

    public Vector3 Au012;
    public Vector3 Au345;

    public AnimationUnits(float au0, float au1, float au2, float au3, float au4, float au5)
    {
        Au012 = new Vector3(au0, au1, au2);
        Au345 = new Vector3(au3, au4, au5);
    }

    public float LipRaiser
    {
        get { return Au012[0]; }
        set { Au012[0] = value; }
    }

    public float JawLowerer
    {
        get { return Au012[1]; }
        set { Au012[1] = value; }
    }

    public float LipStretcher
    {
        get { return Au012[2]; }
        set { Au012[2] = value; }
    }

    public float BrowLowerer
    {
        get { return Au345[0]; }
        set { Au345[0] = value; }
    }

    public float LipCornerDepressor
    {
        get { return Au345[1]; }
        set { Au345[1] = value; }
    }

    public float OuterBrowRaiser
    {
        get { return Au345[2]; }
        set { Au345[2] = value; }
    }

    public float this[int i]
    {
        get
        {
            if (i < 0 || i > MaxNbAnimUnits)
                throw new ArgumentOutOfRangeException("There is only " + MaxNbAnimUnits + " animation units but you requested the nb: " + i);
            if (i < 3)
            {
                return Au012[i];
            }
            return Au345[i - 3];
        }

        set
        {
            if (i < 0 || i > MaxNbAnimUnits)
                throw new ArgumentOutOfRangeException("There is only " + MaxNbAnimUnits + " animation units but you requested the nb: " + i);
            if (i < 3)
            {
                Au012[i] = value;
                return;
            }
            Au345[i - 3] = value;
        }
    }

    public static AnimationUnits operator +(AnimationUnits first, AnimationUnits second)
    {
        var animUnits = new AnimationUnits();
        animUnits.Au012 = first.Au012 + second.Au012;
        animUnits.Au345 = first.Au345 + second.Au345;
        return animUnits;
    }
}