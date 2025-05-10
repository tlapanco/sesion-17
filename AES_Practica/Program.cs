using System;
using System.IO;
using System.Security.Cryptography;


/*      <<Función para generar una clave AES>>      */

static void GenerarClave(string rutaClave)
{
    using (Aes algoritmoAES = Aes.Create())
    {
        algoritmoAES.GenerateKey(); // Generar clave
        File.WriteAllBytes(rutaClave, algoritmoAES.Key); // Guardar clave
        Console.WriteLine("\nClave AES generada y guardada en: " + rutaClave );
    }
}

/*     <<Función para desencriptar un texto>>      */

static byte[] EncriptarTexto(string textoPlano, string rutaClave)
{
    using (Aes algoritmoAES = Aes.Create())
    {
        algoritmoAES.Key = File.ReadAllBytes(rutaClave); // Leer clave
        algoritmoAES.GenerateIV(); // Generar IV
        byte[] iv = algoritmoAES.IV;
        
        using (ICryptoTransform encriptador = algoritmoAES.CreateEncryptor(algoritmoAES.Key, iv))
        using (MemoryStream msEncriptado = new MemoryStream())
        {
            msEncriptado.Write(iv, 0, iv.Length); // Escribir IV al principio
            
            using (CryptoStream csEncriptado = new CryptoStream(msEncriptado, encriptador, CryptoStreamMode.Write))
            using (StreamWriter swEncriptado = new StreamWriter(csEncriptado))
            {
                swEncriptado.Write(textoPlano); // Escribir texto encriptado
            }
            byte[] bytesCifrados = msEncriptado.ToArray();
            string base64Cifrado = Convert.ToBase64String(bytesCifrados);
            Console.WriteLine("\nTexto cifrado (Base64): " + base64Cifrado);
            
            return bytesCifrados;
        }
    }
}

/*     <<Función para desencriptar un texto>>      */

static string DesencriptarTexto(string textoCifrado, string rutaClave, bool esBase64 = true)
{
    byte[] bytesCifrados = Convert.FromBase64String(textoCifrado);
    
    using (Aes algoritmoAES = Aes.Create())
    {
        algoritmoAES.Key = File.ReadAllBytes(rutaClave);

        byte[] iv = new byte[algoritmoAES.BlockSize / 8];
        Array.Copy(bytesCifrados, iv, iv.Length); // Extraer IV

        using (ICryptoTransform desencriptador = algoritmoAES.CreateDecryptor(algoritmoAES.Key, iv))
        using (MemoryStream msDescifrado = new MemoryStream(bytesCifrados, iv.Length, bytesCifrados.Length - iv.Length))
        using (CryptoStream csDescifrado = new CryptoStream(msDescifrado, desencriptador, CryptoStreamMode.Read))
        using (StreamReader srDescifrado = new StreamReader(csDescifrado))
        {
            string textoPlano = srDescifrado.ReadToEnd();
            Console.WriteLine("\nTexto desencriptado: " + textoPlano);
            
            return textoPlano;
        }
    }
}


/*     <<Flujo principal>>     */

string? directorioProyecto = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)?.Parent?.Parent?.Parent?.FullName;

if(directorioProyecto == null) Environment.Exit(1);

string rutaClave = Path.Combine(directorioProyecto, "claveAES.key");

// Generar y guardar la clave
GenerarClave(rutaClave);

// Texto a encriptar
string textoOriginal = "Este es un texto que voy a cifrar";

// Encriptar
byte[] textoEncriptado = EncriptarTexto(textoOriginal, rutaClave);

// Convertir a Base64 para simular entrada externa
string textoBase64 = Convert.ToBase64String(textoEncriptado);

// Desencriptar
DesencriptarTexto(textoBase64, rutaClave);