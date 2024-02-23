/*
  Comunicação e controle WIFI entre software e ESP32-WROOM
  Feito como exemplo para treinar e ter como base para comunicação e controle WIFI.
  ESP32 está no modo servidor e programa se conecta como cliente.
  ESP32 tem IP fixo

  Compilador: Arduino IDE 2.2.1
  Autor: Anderson Iurkiv
  Janeiro 2024
*/


// Bibliotecas
#include <WiFi.h>


// Definições para a configuração do IP
IPAddress ip(192,168,15,101);     // IP do ESP. Tem de colocar um IP válido na faixa do roteador.
IPAddress gateway(192,168,15,1);  // IP do servidor.
IPAddress subnet(255,255,255,0);  // Mascara de rede. Colocar a mesma do roteador.

// Variáveis
const char *ssid = "xxxxxxxxxxx";       // Nome da rede WIFI.
const char *password = "xxxxxxxxxxxx";  // Senha da rede WIFI.
int baudRate = 115200;                  // Velocidade da comunicação.

// Objetos e variáveis
WiFiServer server(80);                  // Declaração do servidor. Parâmetro é o número da porta.
WiFiClient client;                      // Declaração do cliente para aceitar a conexão.
String strRead = "";                    // String de leitura
String strWrite = "";                   // String de escrita

// Declaração de funções
void servidor_escuta_responde();        // Função que controla o recebimento e envio de mensagens.
                                
void setup() 
{
  Serial.begin(baudRate);               // Inicia a serial
}

// Loop infinito
void loop() 
{
  if(WiFi.status() != WL_CONNECTED)     // Aguarda conexão
  {
    Serial.println("\nWiFi desconectado");
    Serial.print("Conectando-se em ");
    Serial.println(ssid);
    WiFi.config(ip, gateway, subnet);   // Configurando o WIFI.
    WiFi.begin(ssid, password);         // Conectando a rede

    while(WiFi.status() != WL_CONNECTED)  // Aguarda conexão.
    {
      delay(1000);                      // Verifica conexão a cada 1s
      Serial.print(".");
    }

    Serial.println("\nWiFi conectado");
    Serial.print("IP: ");
    Serial.println(WiFi.localIP());     // Mostra o endereço IP
    server.begin();                     // Inicializa modo servidor
  }
  servidor_escuta_responde();           // Chama a função de controle das mensagens.
}


/*  Função servidor_escuta_responde()
    Aceita a conexão somente de 1 cliente.
    Recebe uma mensagem, trata e retorna.
*/
void servidor_escuta_responde()
{
  int posRead = 0;
  char aux;
  client = server.available();                        // Aguarda cliente conectar.
  
  if(client)                                          // Se cliente conectado, faça
  {
    Serial.println("Cliente conectado.");
    while(client.connected())                         // Permanece em loop enquanto conectado.
    {
      if(client.available())                          // Se existir dados para ler, faça
      {
        strRead = client.readString();                // Lê uma string de bytes
        strWrite = "Mensagem recebida: " + strRead;   // Monta a string de retorno.
        client.print(strWrite);                       // Retorna a string por WIFI
        Serial.println(strWrite);                     // Retorna a string por Serial
        strRead = "";                                 // Limpa buffer
        strWrite = "";
      }
    }
    client.stop();                                    // Finaliza conexão.
    Serial.println("Cliente desconectado.");
  }
}
