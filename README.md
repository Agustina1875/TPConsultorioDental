# TP-ConsultorioDental
Trabajo práctico grupal sobre un consultorio dental.
Integrantes: 
*Lucia Raising
*Agustina Raising

-INSTRUCCIONES PARA EJECUTAR EL PROYECTO CORRECTAMENTE-
1-Abrir la solución en Visual Studio desde la carpeta del proyecto.
2-Copiar y reemplazar el contenido por esto en appsettings.json dentro del proyecto:

{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=ConsultorioDental;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
  "GoogleKeys": {
    "ClientId": "638385419111-hk1vd7ugef3vgc95e70pmq6egimtvtd1.apps.googleusercontent.com",
    "ClientSecret": "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"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}

3-Ejecuta/compila la aplicación.
4-Inicia sesion con el correo asignado como administrador: 
-Email: admin@consultoriodental.com
-Contraseña: Admin123!

-ACLARACIONES:-
-A partir de esa cuenta ya se puede designar y cambiar roles a nuevos correos.
-La base se genera automáticamente al ejecutar el proyecto con Entity Framework.