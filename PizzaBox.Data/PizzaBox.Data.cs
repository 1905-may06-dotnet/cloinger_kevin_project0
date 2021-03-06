﻿using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PizzaBox.Domain;

namespace PizzaBox.Data
{
    public class Pizza{
        private string crust, size, toppings;
        private decimal cost;
        public Pizza(String size, String crust, String toppings){
            this.size = size;
            this.crust = crust;
            this.toppings = toppings;
            this.cost = BizLogic.PizzaPrice(size, crust, toppings);
        }
        [Key]
        public int PizzaId{get;set;}
        [ForeignKey("OrderId")]
        public int OrderId{get;set;}
        public string Crust {
            get => crust;
            set => crust = value;
        }
        public string Size {
            get => size;
            set => size = value; 
        }
        public decimal Cost {
            get => cost;
        }
        public string Toppings{
            get => toppings;
            set => toppings = value;
        }
    }
    public class User{
        private string email, pass;
        public User(string Email, string Pass){
            this.email = Email;
            this.pass = Pass; 
        }
        public void SaveUser(){
            using(UserDB context = new UserDB())
            {
                context.User.Add(this);
                context.SaveChanges();
            }
        }
        [Key]
        public string Email { get => email; set => email = value; }
        public string Pass { get => pass; set => pass = value; }
        public bool CheckUser(){
            UserDB context = new UserDB();
            var userQuery = 
                from User e in context.User
                where e.email == Email
                select e;
            if(userQuery.FirstOrDefault<User>() != null){
                if(this.pass == userQuery.FirstOrDefault<User>().pass ){
                    return true;
                }else{
                    return false;
                }
            }else{
                return false;
            }
        }
        public List<Order> GetOrders(){
            UserDB context = new UserDB();
            var orderQuery = 
                from Order e in context.Order
                where e.Customer == this
                select e;
            var orders = new List<Order>();
            foreach(Order o in orderQuery){
                orders.Add(o);
            }
            return orders;
        }
        private Order GetLastOrder(){
            UserDB context = new UserDB();
            var orderQuery =
                from Order o in context.Order
                orderby o.Time descending
                select o;
            return orderQuery.FirstOrDefault();
        }
        public bool CanOrder(string location){
            var order = GetLastOrder();
            if(order==null){
                return true;
            }
            if(BizLogic.CheckAllowOrderAtSameLocation(order.Time)&&
               BizLogic.CheckAllowOrderOnlySameLocation(order.Time,location,order.Location)
            ){return true;}
            return false;
        }   
    }
    public class Location{
        private string name;
        public Location(String Name){
            this.name = Name;
        }
        [Key]
        public string Name {get => name; set => name = value;}
        public List<Order> GetOrders(){
            UserDB context = new UserDB();
            var orderQuery =
                from Order o in context.Order
                where o.Location == this.name
                select o;
            var orders = new List<Order>();
            foreach(Order o in orderQuery){
                orders.Add(o);
            }
            return orders;
        }
    }
    public class Order
    {
        public int Id { get=>id; set=>id=value; }
        private int id;
        private string location;
        private User customer;
        private decimal cost;
        private bool confirmed = false;
        public bool Confirmed{get=>confirmed;set=>confirmed=value;}
        public decimal Cost 
        {   get => cost;
            set => cost = cost+value;
        }
        public DateTime Time {get;set;}
        public IList<Pizza> Pizzas {get;set;} = new List<Pizza>();
        public String Location{get => location;set=>location=value;}
        public User Customer{get=>customer;set=>customer=value;}
        private UserDB context = new UserDB();

        //public void Save(){
        //     context.Order.Add(this);
        //     context.User.Attach(this.customer);
        //     context.SaveChanges();
        //     return this.Id;
        // }
        public void Update(){
            context.Order.Update(this);
            context.SaveChanges();
        }
        public List<Pizza> GetPizzas(){
            UserDB context = new UserDB();
            var pizzasQuery =
                from Pizza p in context.Pizza
                where p.OrderId == this.id
                select p;
            var pizzas = new List<Pizza>();
            foreach(Pizza p in pizzasQuery){
                pizzas.Add(p);
            }
            return pizzas;
        }
        public bool CheckPizzaLimits(){
            if(BizLogic.CheckNumberOfPizzas(this.Pizzas.Count))
            {
                return true;
            }
            return false;
        }
        public bool CheckCost(){
            if(BizLogic.CheckMaxCost(this.Cost)){
                return true;
            }
            return false;
        }
    }
    public class Set{
        public static void SetS(){
            BizLogic.SetCommand();
        }
    }
}

//dotnet ef dbcontext scaffold "Server=bobtest.database.windows.net;Database=PizzaBox;user id=kevin;Password=;" Microsoft.EntityFrameworkCore.SqlServer -o test