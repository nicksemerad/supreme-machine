public interface IGateway
{
    public void SendOrder(Order order);

    public void CancelOrder(Order order);

    public void MarketDataTick(Nbbo nbbo);

    event GatewayBase.dgOrderUpdated OnOrderUpdated;
}

/// <summary>
/// 
/// </summary>
public class GatewayBase
{
    public delegate void dgOrderUpdated(Order order, DateTime updateTime);
}

/// <summary>
/// 
/// </summary>
public class Gateway_CBP : IGateway
{
    private CoinbaseProClient coinbaseProClient;

    public Gateway_CBP()
    {
        var authenticator = new Authenticator[Your API Tokens Here]);
        coinbaseProClient = new CoinbaseProClient(authenticator);
    }

    public event GatewayBase.dgOrderUpdated OnOrderUpdated;

    public void CancelOrder(Order order)
    {
        coinbaseProClient.OrdersService.CancelOrder(order.PublicHandlerId);
    }

    public void OnMarketDataTick(Nbbo nbbo)
    {
        //Not used
    }

    public async void SendOrder(Order order)
    {
        await coinbaseProClient.OrdersService.PlaceLimitOrderAsync([order params here]);

    }
} //More complicated method to listen to Coinbase’s websocket for order updates and invokes OnOrderUpdated

/// <summary>
/// 
/// </summary>
public class TestGateway : IGateway
{
    private readonly Dictionary<string, Order> makerOrders = new Dictionary<string, Order>();
    private readonly Dictionary<string, Order> takerOrders = new Dictionary<string, Order>();
    private Nbbo priorNbbo = new Nbbo();

    public event GatewayBase.dgOrderUpdated OnOrderUpdated;


    public void SendOrder(Order order //no market/ limit property of order, yet
    {
        if (order.Price > 0)
            makerOrders[order.OrderId] = order;
        else
            takerOrders[order.OrderId] = order;
    }

    public void CancelOrder(Order order)
    {
        makerOrders.Remove(order.OrderId);
    }
}


