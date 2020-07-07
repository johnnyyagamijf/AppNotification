using AppNotification.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;
using Firebase;
using Firebase.Database.Query;

namespace AppNotification.ViewModels
{
    public class MainPageViewModel : BaseViewModel
    {
        private readonly string ENDERECO_FIREBASE = "https://demoapp-1df56.firebaseio.com/";
        private readonly Firebase.Database.FirebaseClient _firebaseClient;

        private ObservableCollection<Pedido> _pedidos;

        public ObservableCollection<Pedido> Pedidos
        {
            get { return _pedidos; }
            set { _pedidos = value; OnPropertyChanged(); }
        }

        public Pedido PedidoSelecionado;

        public ICommand AceitarPedidoCmd { get; set; }

        public MainPageViewModel()
        {
            _firebaseClient = new Firebase.Database.FirebaseClient(ENDERECO_FIREBASE);
            Pedidos = new ObservableCollection<Pedido>();
            AceitarPedidoCmd = new Command(() => AceitarPedido());
            ListenerPedidos();
        }

        private void AceitarPedido()
        {
            PedidoSelecionado.IdVendedor = 1;
            _firebaseClient
                .Child("pedidos")
                .Child(PedidoSelecionado.KeyPedido)
                .PutAsync(PedidoSelecionado);
        }

        private void ListenerPedidos()
        {
            _firebaseClient
                .Child("pedidos")
                .AsObservable<Pedido>()
                .Subscribe(d =>
                {
                    if (d.EventType == Firebase.Database.Streaming.FirebaseEventType.InsertOrUpdate)
                    {
                        if (d.Object.IdVendedor == 0)
                            AdicionarPedido(d.Key, d.Object);
                        else
                            RemoverPedido(d.Key);
                    }
                    else if (d.EventType == Firebase.Database.Streaming.FirebaseEventType.Delete)
                    {
                        RemoverPedido(d.Key);
                    }
                });
        }

        private void AdicionarPedido(string key, Pedido pedido)
        {
            Pedidos.Add(new Pedido()
            {
                KeyPedido = key,
                Cliente = pedido.Cliente,
                Produto = pedido.Produto,
                Preco = pedido.Preco
            });
        }

        private void RemoverPedido(string pedidoKey)
        {
            var pedido = Pedidos.FirstOrDefault(x => x.KeyPedido == pedidoKey);
            Pedidos.Remove(pedido);
        }
    }
}
