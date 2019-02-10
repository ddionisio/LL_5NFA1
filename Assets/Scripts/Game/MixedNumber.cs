using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct MixedNumber : System.IComparable, System.IComparable<MixedNumber> {
    public int whole;
    public int numerator;
    public int denominator;
    
    public bool isValid { get { return denominator != 0; } }

    public bool isNegative { get { return whole < 0f || numerator < 0f; } }

    public float sign { get { return isNegative ? -1f : 1f; } }

    public float fValue {
        get {
            if(denominator == 0) //fail-safe
                return sign * Mathf.Abs(whole);

            return sign * (Mathf.Abs(whole) + (float)Mathf.Abs(numerator) / denominator);
        }
    }

    /// <summary>
    /// Convert only one whole number to fraction. E.g. [2 1/2] => [1 3/2]
    /// </summary>
    public void WholeToFractionSingle() {
        if(whole == 0)
            return;

        bool _isNegative = isNegative;

        whole = Mathf.Abs(whole);
        numerator = Mathf.Abs(numerator);

        numerator += denominator;
        whole--;

        if(_isNegative) {
            if(whole > 0)
                whole = -whole;
            else
                numerator = -numerator;
        }
    }

    /// <summary>
    /// Convert only a single whole number from numerator. E.g. [5/2] => [1 3/2]
    /// </summary>
    public void FractionToWholeSingle() {
        if(whole == 0)
            return;

        bool _isNegative = isNegative;

        whole = Mathf.Abs(whole);
        numerator = Mathf.Abs(numerator);

        numerator += denominator;
        whole--;

        if(_isNegative) {
            if(whole > 0)
                whole = -whole;
            else
                numerator = -numerator;
        }
    }

    /// <summary>
    /// Convert whole number to fraction. E.g. [1 1/2] => [3/2]
    /// </summary>
    public void WholeToFraction() {
        bool _isNegative = isNegative;

        whole = Mathf.Abs(whole);
        numerator = Mathf.Abs(numerator);

        numerator += whole * denominator;
        whole = 0;

        if(_isNegative)
            numerator = -numerator;
    }

    /// <summary>
    /// Convert numerators to whole, and simplify fraction. E.g. [6/4] => [1 1/2]
    /// </summary>
    public void Simplify() {
        bool _isNegative = isNegative;

        whole = Mathf.Abs(whole);
        numerator = Mathf.Abs(numerator);

        if(numerator > denominator) {
            int amt = Mathf.FloorToInt((float)numerator / denominator);
            whole += amt;
            numerator -= amt * denominator;
        }

        if(_isNegative) {
            if(whole > 0)
                whole = -whole;
            else
                numerator = -numerator;
        }

        int gcf = M8.MathUtil.Gcf(numerator, denominator);

        numerator /= gcf;
        denominator /= gcf;
    }

    public int CompareTo(MixedNumber other) {
        if(fValue < other.fValue)
            return -1;
        else if(fValue > other.fValue)
            return 1;

        return 0;
    }
    
    public override int GetHashCode() {
        return fValue.GetHashCode();
    }

    public override bool Equals(object obj) {
        if(obj is MixedNumber) {
            return fValue == ((MixedNumber)obj).fValue;
        }

        return false;
    }

    public override string ToString() {
        return string.Format("{0} and ({1}/{2})", whole, numerator, denominator);
    }

    int System.IComparable.CompareTo(object obj) {
        if(obj is MixedNumber) {
            var other = (MixedNumber)obj;
            return CompareTo(other);
        }

        return -1;
    }

    public static MixedNumber operator +(MixedNumber a, MixedNumber b) {
        if(a.isValid)
            return b;
        else if(b.isValid)
            return a;

        a.WholeToFraction();
        b.WholeToFraction();

        MixedNumber result;

        if(a.denominator == b.denominator)
            result = new MixedNumber { numerator = a.numerator + b.numerator, denominator = a.denominator };
        else {
            result = new MixedNumber { numerator = a.numerator * b.denominator + b.numerator * a.denominator, denominator = a.denominator * b.denominator };
        }

        return result;
    }

    public static MixedNumber operator -(MixedNumber a, MixedNumber b) {
        if(a.isValid)
            return -b;
        else if(b.isValid)
            return a;

        a.WholeToFraction();
        b.WholeToFraction();

        MixedNumber result;

        if(a.denominator == b.denominator)
            result = new MixedNumber { numerator = a.numerator - b.numerator, denominator = a.denominator };
        else {
            result = new MixedNumber { numerator = a.numerator * b.denominator - b.numerator * a.denominator, denominator = a.denominator * b.denominator };
        }

        return result;
    }

    public static MixedNumber operator-(MixedNumber a) {
        bool isNegative = a.isNegative;

        if(a.whole != 0)
            a.whole = -a.whole;
        else if(a.numerator != 0)
            a.numerator = -a.numerator;

        return a;
    }

    public static bool operator ==(MixedNumber a, MixedNumber b) {
        return a.fValue == b.fValue;
    }

    public static bool operator !=(MixedNumber a, MixedNumber b) {
        return a.fValue != b.fValue;
    }

    public static bool operator <(MixedNumber a, MixedNumber b) {
        return a.fValue < b.fValue;
    }

    public static bool operator >(MixedNumber a, MixedNumber b) {
        return a.fValue < b.fValue;
    }
}
