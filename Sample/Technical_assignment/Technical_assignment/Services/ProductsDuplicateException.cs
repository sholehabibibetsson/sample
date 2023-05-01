namespace Technical_assignment.Services
{
    public class ProductsDuplicateException: Exception
    {
        public ProductsDuplicateException()
      : base("Product ids cannot be the same!!")
        {
        }
    }
}
