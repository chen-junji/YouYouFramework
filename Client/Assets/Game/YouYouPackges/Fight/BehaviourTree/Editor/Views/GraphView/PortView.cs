using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

/// <summary>
/// 行为树端口View
/// </summary>
public class PortView : Port
{
    protected PortView(Orientation portOrientation, Direction portDirection, Capacity portCapacity, Type type) :
        base(portOrientation, portDirection, portCapacity, type)
    {
    }

    public static PortView Create(Orientation orientation, Direction direction, Capacity capacity, Type type)
    {
        EdgeConnectorListener listener = new EdgeConnectorListener();
        var ele = new PortView(orientation, direction, capacity, type);
        ele.m_EdgeConnector = new EdgeConnector<Edge>(listener);
        ele.AddManipulator(ele.m_EdgeConnector);
        return ele;
    }
}