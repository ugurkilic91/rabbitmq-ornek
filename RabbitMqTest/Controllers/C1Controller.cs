using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;
using System.Threading.Channels;

namespace RabbitMqTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class C1Controller : ControllerBase
    {
        public readonly IModel _model;

        public C1Controller(IModel model)
        {
            _model = model;
        }
        [HttpGet()]
        public string Get(string message)
        {
            _model.QueueDeclare("product", exclusive: false);
            //Serialize the message
            //var json = JsonConvert.SerializeObject(message);
            var body = Encoding.UTF8.GetBytes(message);
            //put the data on to the product queue
            //var ttl = new Dictionary<string, object>
            //{
            //    { "x-message-ttl", 3000 },//Kaç saniye sonra kuyrukta işlenmiyorsa sil
            //    {"x-delivery-count",4 }, //consumer da hata varsa deneme sayısı
            //    {"x-dead-letter-exchange","demo-direct-exchange_error" }, //Hata Olursa hangi exchange göndereceğim
            //    {"x-dead-letter-routing-key","demo-direct-exchange_routing-key_error" } //Hata Olursa hangi exchange göndereceğim routing key
            //};
            _model.ExchangeDeclare("demo-direct-exchange", ExchangeType.Direct,arguments:null);
            _model.QueueBind("product", "demo-direct-exchange", "product");
            _model.BasicPublish(exchange: "demo-direct-exchange", routingKey: "product", body: body);
            //_model.BasicPublish(exchange: "demo-direct-exchange", routingKey: "product2", body: body);
            return "Ok";
        }
    }
}
