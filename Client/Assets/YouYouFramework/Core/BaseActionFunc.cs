using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace YouYou
{
    public delegate void BaseAction();

    public delegate void BaseAction<T1>(T1 t1);

    public delegate void BaseAction<T1, T2>(T1 t1, T2 t2);

    public delegate void BaseAction<T1, T2, T3>(T1 t1, T2 t2, T3 t3);

    public delegate void BaseAction<T1, T2, T3, T4>(T1 t1, T2 t2, T3 t3, T4 t4);

    public delegate void BaseAction<T1, T2, T3, T4, T5>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5);

    public delegate void BaseAction<T1, T2, T3, T4, T5, T6>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6);

    public delegate void BaseAction<T1, T2, T3, T4, T5, T6, T7>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7);

    public delegate R BaseFunc<out R>();

    public delegate R BaseFunc<T1, out R>(T1 t1);
}