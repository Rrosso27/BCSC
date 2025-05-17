using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private const string FixedPassword = "admin"; // Contraseña fija para todos los usuarios

    private readonly IServicioEmail servicioEmail;

    public UserController(IServicioEmail servicioEmail)
    {
        this.servicioEmail = servicioEmail;
    }
    /// <summary>
    /// Permite a un usuario iniciar sesión con su UserId y una contraseña fija.
    /// </summary>
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        // Verificar si el usuario existe
        var user = TokenSystem.GetUser(request.UserId);
        if (user == null)
        {
            return Unauthorized("Invalid UserId.");
        }

        // Verificar la contraseña
        if (request.Password != FixedPassword)
        {
            return Unauthorized("Invalid password.");
        }

        // Retornar una respuesta exitosa
        return Ok(new { Message = "Login successful", UserId = user.Id });
    }

    /// <summary>
    /// Registra un nuevo usuario con su UserId y Email.
    /// </summary>
    /// </summary>
    [HttpPost("register")]
    public async Task<ActionResult> RegisterUser([FromBody] RegisterUserRequest request)
    {
        try
        {
            var existingUser = TokenSystem.GetUser(request.UserId);
            if (existingUser != null)
                return BadRequest("User already exists.");

            TokenSystem.AddUser(request.UserId, request.Email);

            // Enviar correo y guardar el código en el usuario
            var code = servicioEmail.SendVerificationEmail(request.Email);
            var user = TokenSystem.GetUser(request.UserId);
            user.VerificationCode = code;

            return Ok($"User {request.UserId} registered successfully. Verification code sent to email.");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("verify")]
    public IActionResult VerifyUser([FromBody] VerifyUserRequest request)
    {
        var user = TokenSystem.GetUser(request.UserId);
        if (user == null)
            return NotFound("User not found.");

        // Acepta cualquier código de la lista
        if (servicioEmail.GetCodigosPosibles().Contains(request.Code))
        {
            user.IsVerified = true;
            return Ok("User verified successfully.");
        }
        else
        {
            return BadRequest("Invalid verification code.");
        }
    }
}

