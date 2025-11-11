namespace GameBerry.Event
{
    public class GetAllGameChartResponseMsg : Message
    {
        public bool IsSuccess = false;
    }

    public class CompleteTableLoadMsg : Message
    {
        public bool IsSuccess = false;
    }
}