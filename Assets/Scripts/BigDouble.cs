using System;
using UnityEngine;

[System.Serializable]
public struct BigDouble : IComparable<BigDouble>
{
    public double mantissa;
    public int exponent;

    public BigDouble(double value)
    {
        if (value == 0)
        {
            mantissa = 0;
            exponent = 0;
            return;
        }

        exponent = (int)Math.Floor(Math.Log10(Math.Abs(value)));
        mantissa = value / Math.Pow(10, exponent);
        Normalize();
    }

    public BigDouble(double mantissa, int exponent)
    {
        this.mantissa = mantissa;
        this.exponent = exponent;
        Normalize();
    }

    private void Normalize()
    {
        if (mantissa == 0)
        {
            exponent = 0;
            return;
        }

        while (Math.Abs(mantissa) >= 10)
        {
            mantissa /= 10;
            exponent++;
        }

        while (Math.Abs(mantissa) < 1 && mantissa != 0)
        {
            mantissa *= 10;
            exponent--;
        }
    }

    public static BigDouble operator +(BigDouble a, BigDouble b)
    {
        if (a.mantissa == 0) return b;
        if (b.mantissa == 0) return a;

        int expDiff = a.exponent - b.exponent;
        if (expDiff > 15) return a;
        if (expDiff < -15) return b;

        double newMantissa = a.mantissa * Math.Pow(10, expDiff) + b.mantissa;
        return new BigDouble(newMantissa, b.exponent);
    }

    public static BigDouble operator -(BigDouble a, BigDouble b)
    {
        if (b.mantissa == 0) return a;
        if (a.mantissa == 0) return new BigDouble(-b.mantissa, b.exponent);

        int expDiff = a.exponent - b.exponent;
        if (expDiff > 15) return a;
        if (expDiff < -15) return new BigDouble(-b.mantissa, b.exponent);

        double newMantissa = a.mantissa * Math.Pow(10, expDiff) - b.mantissa;
        return new BigDouble(newMantissa, b.exponent);
    }

    public static BigDouble operator *(BigDouble a, BigDouble b)
    {
        return new BigDouble(a.mantissa * b.mantissa, a.exponent + b.exponent);
    }

    public static BigDouble operator /(BigDouble a, BigDouble b)
    {
        if (b.mantissa == 0)
            throw new DivideByZeroException();
        return new BigDouble(a.mantissa / b.mantissa, a.exponent - b.exponent);
    }

    public static bool operator >(BigDouble a, BigDouble b)
    {
        if (a.exponent != b.exponent)
            return a.exponent > b.exponent;
        return a.mantissa > b.mantissa;
    }

    public static bool operator <(BigDouble a, BigDouble b)
    {
        if (a.exponent != b.exponent)
            return a.exponent < b.exponent;
        return a.mantissa < b.mantissa;
    }

    public static bool operator >=(BigDouble a, BigDouble b)
    {
        return a > b || a == b;
    }

    public static bool operator <=(BigDouble a, BigDouble b)
    {
        return a < b || a == b;
    }

    public static bool operator ==(BigDouble a, BigDouble b)
    {
        return a.exponent == b.exponent && Math.Abs(a.mantissa - b.mantissa) < 1e-10;
    }

    public static bool operator !=(BigDouble a, BigDouble b)
    {
        return !(a == b);
    }

    public int CompareTo(BigDouble other)
    {
        if (this > other) return 1;
        if (this < other) return -1;
        return 0;
    }

    public override bool Equals(object obj)
    {
        if (obj is BigDouble)
            return this == (BigDouble)obj;
        return false;
    }

    public override int GetHashCode()
    {
        return mantissa.GetHashCode() ^ exponent.GetHashCode();
    }

    public double ToDouble()
    {
        if (exponent > 308)
            return double.PositiveInfinity;
        if (exponent < -308)
            return 0;
        return mantissa * Math.Pow(10, exponent);
    }

    public override string ToString()
    {
        if (mantissa == 0)
            return "0";

        if (exponent < 6 && exponent > -3)
        {
            double value = ToDouble();
            // 정수인 경우 소수점 생략
            if (Math.Abs(value - Math.Round(value)) < 0.001)
            {
                return value.ToString("F0");
            }
            return value.ToString("F2");
        }

        return $"{mantissa:F2}e{exponent}";
    }

    public static BigDouble Pow(BigDouble baseValue, double power)
    {
        double newExponent = baseValue.exponent * power;
        double newMantissa = Math.Pow(baseValue.mantissa, power);

        int expInt = (int)newExponent;
        double expFrac = newExponent - expInt;

        newMantissa *= Math.Pow(10, expFrac);

        return new BigDouble(newMantissa, expInt);
    }

    public static BigDouble Zero => new BigDouble(0);
    public static BigDouble One => new BigDouble(1);
    public static BigDouble Infinity => new BigDouble(1.79, 308);
}
