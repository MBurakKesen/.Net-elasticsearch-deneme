using Elastic.Clients.Elasticsearch;

namespace elasticsearch_deneme
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddAuthorization();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();


            ElasticsearchClientSettings settings = new(new Uri("http://localhost:9200"));
            settings.DefaultIndex("products");

            ElasticsearchClient client = new(settings);
            client.IndexAsync("products").GetAwaiter().GetResult();


            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapPost("/products/create", async (CreateProductDto request, CancellationToken token) =>
            {
                Product product = new()
                {
                    Id = Guid.NewGuid(),
                    Description = request.Description,
                    Name = request.Name,
                    Stock = request.Stock,
                    Price = request.Price
                };
                CreateRequest<Product> createRequest = new(product.Id.ToString())
                {
                    Document = product
                }; 
                CreateResponse createResponse = await client.CreateAsync(createRequest, token);
                return Results.Ok(createResponse.Id);
            });

            app.MapGet("/products/getAll", async (CancellationToken token) =>
            {
                var searchRequest = new SearchRequest<Product>("products");

                SearchResponse<Product> response = await client.SearchAsync<Product>(searchRequest, token);

                return Results.Ok(response.Documents);

            });

            app.MapPut("/products/update", async (UpdateProductDto request, CancellationToken cancellationToken) =>
            {

                UpdateRequest<Product, UpdateProductDto> updateRequest = new("products", request.Id.ToString())
                {
                    Doc = request
                };

                UpdateResponse<Product> updateResponse = await client.UpdateAsync(updateRequest, cancellationToken);

                return Results.Ok(new { message = "Update is successful" });
            });

            app.MapDelete("/products/deleteById", async (Guid id, CancellationToken cancellationToken) =>
            {
                DeleteResponse deleteResponse = await client.DeleteAsync("products", id, cancellationToken);

                return Results.Ok(new { message = "Delete is successful" });
            });

            app.Run();
        }
    }
    public class Product
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string Description { get; set; }
    }
    public record CreateProductDto(
        string Name,
        decimal Price,
        int Stock,
        string Description
        );

    public record UpdateProductDto(
        Guid Id,
        string Name,
        decimal Price,
        int Stock,
        string Description
        );


}