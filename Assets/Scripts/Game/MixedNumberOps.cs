﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MixedNumberOps {
    [System.Serializable]
    public class Operand {
        public MixedNumber[] numbers; //set to > 0 to have this as a fixed number, otherwise ApplyNumber needs to be called.

        public MixedNumber number {
            get {
                if(numbers.Length > 0) {
                    if(numbers.Length == 1)
                        return numbers[0];
                    else {
                        if(mNumberIndex == -1)
                            mNumberIndex = Random.Range(0, numbers.Length);

                        return numbers[mNumberIndex];
                    }
                }
                else
                    return mNumber;
            }
        }
        
        public bool isFixed { get { return numbers.Length > 0; } }
        public bool isEmpty { get { return numbers.Length == 0 && mIsEmpty; } }

        private MixedNumber mNumber;
        private int mNumberIndex = -1;
        private bool mIsEmpty = true;

        public void ApplyNumber(MixedNumber toNumber) {
            mNumber = toNumber;
            mIsEmpty = false;
        }

        public void RemoveNumber() {
            mIsEmpty = true;
        }

        public Operand Clone() {
            var ret = new Operand();
            ret.numbers = new MixedNumber[numbers.Length];
            System.Array.Copy(numbers, ret.numbers, numbers.Length);

            ret.mNumber = mNumber;
            ret.mNumberIndex = mNumberIndex;
            ret.mIsEmpty = mIsEmpty;
            
            return ret;
        }
    }

    public Operand[] operands; //at least two
    public OperatorType[] operators; //must be one less the number of operands

    public bool isAnyOperandEmpty {
        get {
            for(int i = 0; i < operands.Length; i++) {
                if(operands[i].isEmpty)
                    return true;
            }

            return false;
        }
    }

    public MixedNumberOps Clone(int operandCount) {
        var ret = new MixedNumberOps();

        int _operandCount = Mathf.Min(operandCount, operands.Length);
        int _operatorCount = _operandCount > 1 ? Mathf.Min(_operandCount - 1, operators.Length) : 0;

        ret.operands = new Operand[_operandCount];
        for(int i = 0; i < _operandCount; i++)
            ret.operands[i] = operands[i].Clone();

        ret.operators = new OperatorType[_operatorCount];
        System.Array.Copy(operators, ret.operators, _operatorCount);

        return ret;
    }

    public void ApplyOperand(int index, MixedNumber number) {
        if(index < 0 || index >= operands.Length) {
            Debug.LogWarning("Invalid operand index: " + index);
            return;
        }

        if(operands[index].isFixed) {
            Debug.LogWarning("Operand is fixed at index: " + index);
            return;
        }

        operands[index].ApplyNumber(number);
    }

    public void RemoveOperand(int index) {
        if(index < 0 || index >= operands.Length) {
            Debug.LogWarning("Invalid operand index: " + index);
            return;
        }

        if(operands[index].isFixed) {
            Debug.LogWarning("Operand is fixed at index: " + index);
            return;
        }

        operands[index].RemoveNumber();
    }

    public MixedNumber Evaluate() {
        //fail-safe
        if(operands.Length == 0)
            return new MixedNumber();
        else if(operands.Length == 1)
            return operands[0].number;

        MixedNumber result = operands[0].isEmpty ? new MixedNumber() : operands[0].number;

        for(int i = 0; i < operators.Length; i++) {
            var op2 = operands[i + 1];
            if(op2.isEmpty)
                continue;

            switch(operators[i]) {
                case OperatorType.Add:
                    result += op2.number;
                    break;
                case OperatorType.Subtract:
                    result -= op2.number;
                    break;
            }
        }

        return result;
    }
}
