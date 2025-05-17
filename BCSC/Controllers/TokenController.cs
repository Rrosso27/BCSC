using Microsoft.AspNetCore.Mvc;
using System.Transactions;

[ApiController]
[Route("api/[controller]")]
public class TokenController : ControllerBase
{
    /// <summary>
    /// Obtiene el valor actual de los tokens basado en la oferta y la demanda.
    /// </summary>
    [HttpGet("value")]
    public IActionResult GetTokenValue()
    {
        var value = TokenSystem.GetTokenValue();
        return Ok(new { Value = value });
    }

    private bool IsRegisteredUser(string userId)
    {
        return TokenSystem.GetUser(userId) != null;
    }

    /// <summary>
    /// Distribuye una cantidad específica de tokens a un usuario.
    /// </summary>
    [HttpPost("distribute")]
    public IActionResult DistributeTokens([FromBody] DistributeRequest request)
    {
        if (!IsRegisteredUser(request.UserId))
            return Unauthorized("User is not registered.");

        try
        {
            if (!IsBurnedCreditCard(request.CreditCardNumber, request.ExpirationDate, request.CVV))
            {
                return BadRequest("The provided credit card is not valid, expired, or not burned.");
            }

            decimal tokenValue = TokenSystem.GetTokenValue();
            decimal totalCost = request.Amount * tokenValue;

            TokenSystem.DistributeInitialTokens(request.UserId, request.Amount);

            var blockchain = TokenSystem.GetBlockchain();
            var newBlock = blockchain.GetLatestBlock();
            NodeManager.PropagateBlock(newBlock);

            return Ok($"Successfully purchased {request.Amount} tokens for {totalCost} using a burned credit card.");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }


    /// <summary>
    /// Transfiere una cantidad específica de tokens de un usuario a otro.
    /// </summary>
    [HttpPost("transfer")]
    public IActionResult TransferTokens([FromBody] TransferRequest request)
    {
        if (!IsRegisteredUser(request.SenderId) || !IsRegisteredUser(request.ReceiverId))
            return Unauthorized("Sender or receiver is not registered.");

        try
        {
            TokenSystem.TransferTokens(request.SenderId, request.ReceiverId, request.Amount);
            return Ok($"Transferred {request.Amount} tokens from {request.SenderId} to {request.ReceiverId}.");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }


    /// <summary>
    /// Obtiene el balance actual de tokens de un usuario.
    /// </summary>
    [HttpGet("balance/{userId}")]
    public IActionResult GetBalance(string userId)
    {
        if (!IsRegisteredUser(userId))
            return Unauthorized("User is not registered.");

        var user = TokenSystem.GetUser(userId);

        var tokenValue = TokenSystem.GetTokenValue();

        return Ok(new
        {
            UserId = user.Id,
            Balance = user.Balance,
            TokenValue = tokenValue
        });
    }

    [HttpPost("sell")]
    public IActionResult SellTokens([FromBody] SellRequest request)
    {
        try
        {
            TokenSystem.SellTokens(request.UserId, request.Amount);
            return Ok($"User {request.UserId} sold {request.Amount} tokens successfully.");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }


    /// <summary>
    /// Obtiene un resumen del sistema de tokens, incluyendo los tokens disponibles, distribuidos y los balances de los usuarios.
    /// </summary>
    [HttpGet("summary")]
    public IActionResult GetTokenSummary()
    {
        var summary = TokenSystem.GetTokenSummary();
        return Ok(summary);
    }

    private bool IsBurnedCreditCard(string creditCardNumber, string expirationDate, string cvv)
    {
        // Validar si la tarjeta es "quemada" (por ejemplo, empieza con "9999")
        if (!creditCardNumber.StartsWith("9999"))
        {
            return false;
        }

        // Validar la fecha de vencimiento (formato MM/YY)
        if (!DateTime.TryParseExact(expirationDate, "MM/yy", null, System.Globalization.DateTimeStyles.None, out var expDate))
        {
            return false;
        }

        if (expDate < DateTime.UtcNow)
        {
            return false; // La tarjeta está vencida
        }

        // Validar el CVV (3 o 4 dígitos)
        if (cvv.Length < 3 || cvv.Length > 4 || !cvv.All(char.IsDigit))
        {
            return false;
        }

        return true;
    }

}
