using Login.Models;
using Login.Recursos;
using Login.Servicios.Contrato;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Login.Controllers
{
    public class InicioController : Controller
    {
        private readonly IUsuarioService _usuarioServicio;
        public InicioController(IUsuarioService usuarioServicio)
        {
            _usuarioServicio = usuarioServicio;
        }

        public IActionResult Registrarse()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Registrarse(Usuario modelo)
        {
            // Verificar si el correo tiene una de las terminaciones permitidas
            if (!modelo.Correo.EndsWith("@ucc.edu.co") && !modelo.Correo.EndsWith("@campusucc.edu.co"))
            {
                // Mostrar mensaje de error y redirigir a la vista de registro
                ViewData["Mensaje"] = "Solo te puedes registrar con tu correo institucional";
                return View();
            }

            // Encriptar la clave
            modelo.Clave = Utilidades.EncriptarClave(modelo.Clave);

            // Guardar el usuario en la base de datos
            Usuario usuario_creado = await _usuarioServicio.SaveUsuario(modelo);

            // Verificar si el usuario se creó correctamente
            if (usuario_creado.IdUsuario > 0)
                return RedirectToAction("IniciarSesion", "Inicio");

            // Mostrar mensaje de error si no se pudo crear el usuario
            ViewData["Mensaje"] = "No se pudo crear el usuario";
            return View();
        }

        public IActionResult IniciarSesion()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> IniciarSesion(string correo, string clave)
        {

            Usuario usuario_encontrado = await _usuarioServicio.GetUsuario(correo, Utilidades.EncriptarClave(clave));

            if (usuario_encontrado == null)
            {
                ViewData["Mensaje"] = "No se encontraron coincidencias";
                return View();
            }

            List<Claim> claims = new List<Claim>() {
                new Claim(ClaimTypes.Name, usuario_encontrado.NombreUsuario)
            };

            ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            AuthenticationProperties properties = new AuthenticationProperties()
            {
                AllowRefresh = true
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                properties
                );

            return RedirectToAction("Index", "Home");
        }
    }
}