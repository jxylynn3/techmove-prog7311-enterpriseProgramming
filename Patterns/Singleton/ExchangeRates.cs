namespace ST10448420_TechMove_GLMS.Patterns.Singleton
{
    public class ExchangeRates
    {
        private static ExchangeRates _instance;
        private static readonly object _lock = new object();// this is used to that only one thread can access the instance creation code at a time, preventing multiple instances from being created in a multi-threaded environment.
        private decimal _rate = 16.5m; // fallback/demo
        private ExchangeRates()=> Console.WriteLine("ExchangeRates instance created.");

        public static ExchangeRates Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                        _instance = new ExchangeRates();

                    return _instance;
                }
            }
        }

        public decimal GetRate()
        {
            return _rate;
        }
    }
}// used for the API logic to get the exchange rate for currency conversion in the ServiceRequestBuilder.
 // By using the Singleton pattern, we ensure that there is only one instance of the ExchangeRates class throughout the application, which can be accessed globally.