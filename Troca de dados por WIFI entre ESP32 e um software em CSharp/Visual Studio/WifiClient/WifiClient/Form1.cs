using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

/*
 *   Comunicação e controle WIFI entre software e ESP32-WROOM
 *   Feito como exemplo para treinar e ter como base para comunicação e controle WIFI.
 *   Software é configurado como cliente.
 *   
 *   Compilador: Visual Studio Community 2022 - 17.6.5
 *   Autor: Anderson Iurkiv
 *   Janeiro 2024
 */
namespace WifiClient
{
    public partial class Form1 : Form
    {
        // Objetos e variáveis
        private TcpClient client;
        private NetworkStream stream;
        private bool conectado = false;

        public Form1()
        {
            InitializeComponent();

            // Iniciando variáveis globais.
            textBoxIP.Text = "xxx.xxx.xx.xxx:xx";	// Inserir aqui IP:porta
            textBoxTempo.Text = "5000";
            textBoxRecebido.Enabled = false;
            client = null;
            stream = null;
        }

        /*  Evento buttonConectar_Click()
         *  Controla a ação de clicar no botão conectar. 
         */
        private void buttonConectar_Click(object sender, EventArgs e)
        {
            // Se não estiver conectado, conecta.
            if (!conectado)
            {
                // Só tenta conectar se o campo com o IP de conexão não for nulo.
                if (!string.IsNullOrEmpty(textBoxIP.Text))
                {
                    // Configura botões, campos ...
                    client = new TcpClient();
                    textBoxIP.Enabled = false;
                    textBoxTempo.Enabled = false;
                    buttonConectar.Enabled = false;
                    buttonConectar.Text = "Conectando";
                    string[] strCon = textBoxIP.Text.Split(':');            // Separa a string em IP e porta
                    client.Connect(strCon[0], Convert.ToInt32(strCon[1]));  // Conecta
                    // Se conectar
                    if (client.Connected)
                    {
                        stream = client.GetStream();
                        buttonConectar.Enabled = true;
                        buttonConectar.Text = "Desconectar";
                        conectado = true;
                    }
                    // Se não conectar, avisa e reconfigura botões e campos.
                    else
                    {
                        buttonConectar.Enabled = true;
                        buttonConectar.Text = "Conectar";
                        textBoxTempo.Enabled = true;
                        textBoxRecebido.Text = "Erro ao conectar.";
                        client.Close();
                        client = null;
                    }   
                }
            }
            // Se não, desconecta
            else
            {
                buttonConectar.Enabled = false;
                buttonConectar.Text = "Desconectando";
                client.Close();
                stream.Close();
                client = null;
                stream = null;
                textBoxIP.Enabled = true;
                buttonConectar.Enabled = true;
                textBoxTempo.Enabled = true;
                buttonConectar.Text = "Conectar";
                conectado = false;
            }
        }

        /*  Evento buttonEnviar_Click()
         *  Controla a ação de enviar dados. 
         */
        private void buttonEnviar_Click(object sender, EventArgs e)
        {
            // Só envia se estiver conectado
            if (conectado)
            {
                byte[] dados = Encoding.ASCII.GetBytes(textBoxEnviar.Text);
                stream.Write(dados, 0, dados.Length);
                textBoxEnviar.Clear();
                textBoxRecebido.Clear();

                // Abre a função que aguarda o recebimento da mensagem em outra thread.
                Task.Run(() => aguarda_receber_dados(Convert.ToInt64(textBoxTempo.Text)));
            }
        }

        /*  Função aguarda_receber_dados()
         *  Aguarda é retorno da mensagem até o tempo configurado no textBoxTempo 
         */
        private void aguarda_receber_dados(long tempo)
        {
            bool recebido = false;
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            while(stopWatch.ElapsedMilliseconds < tempo)
            {
                if (stream.DataAvailable)
                {
                    stopWatch.Stop();
                    // Como abro a função em uma thread, tenho de usar delegate para alterar o conteúdo do textBox
                    Invoke(new Action (() => atualiza_textBoxRecebido(stopWatch.ElapsedMilliseconds)));
                    recebido = true;
                    break;
                }
                Thread.Sleep(100);
            }
            if(!recebido)
            {
                stopWatch.Stop();
                Invoke(new Action(() => textBoxRecebido.Text = "Tempo expirou"));
            }
        }

        /*  Função atualiza_textBoxRecebido()
         *  Altera o conteúdo do textBoxRecebido.
         *  Recebe como parâmentro o tempo de retorno em ms
         */
        private void atualiza_textBoxRecebido(long tempo)
        {
            byte[] dados = new byte[100];
            int tam = stream.Read(dados, 0, 100);
            string str = "Tempo: " + tempo.ToString() + " ms - " + Encoding.ASCII.GetString(dados, 0, tam);
            textBoxRecebido.Text = str;
        }

        /*  Função Form1_FormClosing()
         *  Garante que desconecta antes de fechar o programa.
         */
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (client != null)
                if (client.Connected)
                    client.Close();
        }

        /*  Função buttonLimpar_Click()
         *  Limpa o textBoxRecebido
         */
        private void buttonLimpar_Click(object sender, EventArgs e)
        {
            textBoxRecebido.Clear();
        }
    }
}
