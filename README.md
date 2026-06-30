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

3-Restaura la base de datos con el archivo .bak dentro de la carpeta.
      (si no funciona podemos crearla nuevamente de 0 con el archivo setup)
4-Ejecuta/compila la aplicación.

-PARA ASIGNAR POR PRIMERA VEZ EL ROL DE ADMINISTRADOR:-
1.Ejecutar la aplicación.
2.Iniciar sesión con Google utilizando la cuenta que será Administrador.
3.Abri SQL server management studio y conectate a (localdb)\MSSQLLocalDB.
4.Fijate si estas en la base adecuada (ConsultorioDental) y hace click en "New Query".
5.Pega y ejecuta el siguiente script reemplazando el correo con el que iniciaste sesión:

DECLARE @Email NVARCHAR(256) = 'Tucorreo@gmail.com';

INSERT INTO AspNetUserRoles (UserId, RoleId)
SELECT
    U.Id,
    R.Id
FROM AspNetUsers U
CROSS JOIN AspNetRoles R
WHERE U.Email = @Email
  AND R.Name = 'Administrador'
  AND NOT EXISTS
  (
      SELECT 1
      FROM AspNetUserRoles UR
      WHERE UR.UserId = U.Id
        AND UR.RoleId = R.Id
  );

5-Cerrar sesion y volver a ingresar, ya ingresas con el rol admin. 

-ACLARACIONES:-
-A partir de esa cuenta ya se puede designar y cambiar roles a nuevos correos.
