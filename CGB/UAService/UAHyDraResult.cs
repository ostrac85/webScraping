namespace CGB.UAService
{
    public class UAHyDraResult<T> where T : new()
    {
        public string Msg { get; set; }
        public long Status { get; set; }
        public T Data { get; set; }
    }
}
