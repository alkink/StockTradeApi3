using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using StockTradeApi3.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace StockTradeApi3.Controllers
{

    [Authorize(Roles ="admin")]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private Db1Context db1;
        public UserController(Db1Context db)

        {
            db1 = db;
        }


        
        [HttpGet]
        public List<User> GetUser()
        {
            List<User> users = db1.Users.ToList();
            return users;
        }
        
        [HttpPost]
        
        public User Add([FromBody] User user)
        {
            User user2 = user;
            db1.Users.Add(user2);
            db1.SaveChanges();
            return user;
        }

        [HttpPut]
        public User? EditUser([FromBody] User user)
        {
            User user2 = db1.Users.FirstOrDefault(x => x.Id == user.Id)!;
            if (user2!= null) {
                user2.Username = user.Username;
                user2.Password = user.Password;
                user2.Balance = user.Balance;
                if (user.Role == "admin" || user.Role == "user")
                {
                    user2.Role = user.Role;
                }
                else
                {
                    return null;
                }
                db1.SaveChanges();
                return user;
            }
            return null;
            
        }
        [HttpDelete]
        public User? DeleteUser(int id)
        {
            User user = db1.Users.Find(id)!;
             db1.Users.Remove(user);
            db1.SaveChanges();
            return user;
        }
        [HttpPost("setcomissionrate")]
        public void SetcomissionRate(int rate)
        {
            db1.CommissionRates.FirstOrDefault().AdminCommissionRate = rate;
        }
        [HttpPost("createbalancecard")]
        public void CreateBalanceCard(BalanceCard balanceCard)
        {
            balanceCard.IsUsed = false;
            db1.BalanceCards.Add(balanceCard);
        }
        [HttpPost("TopUpBalance")]
        public void TopUpBalance(BalanceCard balanceCard)
        {
            string? userNameToken = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            User user = db1.Users.FirstOrDefault(x => x.Username == userNameToken)!;
            BalanceCard balanceCard2 = db1.BalanceCards.FirstOrDefault(x=>x.CardId == balanceCard.CardId)!;
            if (balanceCard2.IsUsed == false)
            {
                user.Balance += balanceCard2.BalanceAmount;
            }
            balanceCard2.IsUsed = true;
        }

    }
}

