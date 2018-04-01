using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SQLite;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Collections;


namespace ThriftShop
{
    class Program
    {
        public SQLiteConnection sqlite_con;
        public string db_src = "Data Source=C:/Users/Soha Samad/Desktop/College/year 3/First Semester/Concepts of programming lang/Labs/Assign#3/Q2-c#/products.db";
        
        public class product
        {
            public long id;
            public string name;
            public long price;
            public string category;
            public long brand_id;


           public  product(long id,string name,long price,string category,long brand_id)
            {
                this.id = id;
                this.name = name;
                this.price = price;
                this.category = category;
                this.brand_id = brand_id;
            }
            void set_id(int id) { this.id = id; }
            long get_id() { return this.id; }
            void set_name(string name) { this.name = name; }
            string get_name() { return this.name;}
            void set_price(long price) { this.price = price; }
            long get_price() { return this.price; }
            void set_category(string category) { this.category = category; }
            string get_category() { return this.category; }
            void set_brandid(int brand_id) { this.brand_id = brand_id; }
            long get_brandid() { return this.brand_id; }
        }

        public class brand
        {
            public long id;
            public string brand_name;
            public brand(long id,string brand_name)
            {
                this.id = id;
                this.brand_name = brand_name;
            }
            void set_id(long id) { this.id = id; }
            long get_id() { return this.id; }
            void set_name(string name) { this.brand_name = name; }
            string get_name() { return this.brand_name; }

        }


        public List<product> p = new List<product>(); //making a list of products.
        public List<brand> b = new List<brand>(); //making a list of brands.
        public void mappping() //to map the data in the database to objects.
        {

            try
            {
                sqlite_con = new SQLiteConnection(db_src);
                sqlite_con.Open();
                var context = new DataContext(sqlite_con);

                string sql = "select * from Product ";
                SQLiteCommand command = new SQLiteCommand(sql, sqlite_con);
                SQLiteDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    p.Add(new product((long)reader["Id"], (string)reader["Name"], (long) reader["Price"], (string)reader["Category"], (long)reader["Brand_id"]));

                }
                sql = "select * from Brand";
                command = new SQLiteCommand(sql, sqlite_con);
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    b.Add(new brand((long)reader["Id"], (string)reader["BrandName"]));
                    
                }
                command.Dispose();

            }

            catch (SQLiteException ) { Console.WriteLine("Exception caught in sql connection handling "); }
            catch (System.InvalidOperationException) { Console.WriteLine("Invalid operation"); }

        }
       

        public void  print_database()
        {
            foreach(product pro in p)
            {
                Console.Write(pro.id.ToString() + " ");
                Console.Write(pro.name + " ");
                Console.Write(pro.price.ToString() + " ");
                Console.Write(pro.category + " ");
                Console.Write(pro.brand_id.ToString() + " ");
                Console.WriteLine();
            }
            Console.WriteLine();
            foreach (brand br in b)
            {
                Console.WriteLine(br.brand_name, " ");
            }
                


        }

        public void show_products()
        {
            var query = from pro in p
                        join br in b
                        on pro.brand_id equals br.id
                        select new { pro.name, pro.category, pro.price,  br.brand_name };

            string header = String.Format("{0,-15} {1,15} {2,15} {3,15} \n", "PRODUCT NAME", "PRICE", "CATEGORY", "BRAND NAME");
            Console.WriteLine(header);
            foreach (var group in query)
            {
                string output = String.Format("{0,-15} {1,15} {2,15} {3,15} \n",group.name, group.price, group.category, group.brand_name);
                Console.WriteLine(output);

            }
        }

        public void add_product( string name ,long price ,string category , string brandname) // function to add product to db.
        {
            sqlite_con = new SQLiteConnection(db_src);
            sqlite_con.Open();
            SQLiteTransaction tran = sqlite_con.BeginTransaction();
            string sql = "select Brand.Id from Brand where Brand.BrandName= @brandname ";
            SQLiteCommand command = new SQLiteCommand(sql, sqlite_con);
            command.Parameters.AddWithValue("@brandname", brandname);
            SQLiteDataReader reader = command.ExecuteReader();
            long brand_id=0;
            bool flag = false;
            while (reader.Read())
            { brand_id = (long)reader["Id"]; flag = true; }
            
            if(flag) // if there is a brand for the product given then add it .
            {
                sql = "insert into Product (Name,Price,Category,Brand_id) values (@name ,@price ,@category , @brand_id )";
                command = new SQLiteCommand(sql, sqlite_con);
                command.Parameters.AddWithValue("@name", name);
                command.Parameters.AddWithValue("@price", price);
                command.Parameters.AddWithValue("@category", category);
                command.Parameters.AddWithValue("@brand_id", brand_id);

                int result = command.ExecuteNonQuery();
                if (result != 0) { Console.WriteLine("Insertion done succesfully"); }
                tran.Commit();

                sql = "select Product.Id from Product where Product.Name=@name";
                command = new SQLiteCommand(sql, sqlite_con);
                command.Parameters.AddWithValue("@name",name);
                reader=command.ExecuteReader();
                while (reader.Read())
                { p.Add(new product((long)reader["Id"], name,(long) price, category, brand_id)); }

                sqlite_con.Dispose();
                
                
            }
            else
            { Console.WriteLine("The brand for this product isnt available "); }


        }
       

        public void filter_product(long price) //filtering product by price less than the given one.
        {
            var query = from pro in p
                        where pro.price < price
                        select new { pro.name, pro.price, pro.category };
                       
            foreach ( var group in query )
            {
                string output = String.Format("{0,-20} {1,20} {2,20} \n", group.name, group.price, group.category);
                Console.WriteLine(output);
            }
            

        }

        public void show_brands()
        {
            var query = from pro in p
                        join br in b
                        on pro.brand_id equals br.id
                        group pro by pro.brand_id into brandcnt
                        orderby brandcnt.Count()
                        select new { brandcnt.First().brand_id ,count=brandcnt.Count() , };


          foreach(var group in query )
          {            
              long id=group.brand_id;
              var brandname = from br in b
                                 where br.id.Equals(id)
                                 select new { br.brand_name };
              foreach (var bn in brandname)
              { 
                 string output= String.Format(" {0,-10} {1,10} \n" ,bn.brand_name.ToString(),  group.count );
                  Console.WriteLine(output);
              }
              
          }
            


        }

        void sort_products (int x ,int y) // sort x : by name ,price (1,2)  --> sort y:  sorting direction (1,2) asc , desc
        {

            if(x==1)
            {
                if(y==1)
                {
                    var query = from pro in p
                                join br in b
                                on pro.brand_id equals br.id
                                orderby pro.name ascending
                                select new { pro.name, pro.category, pro.price, br.brand_name };

                    foreach (var group in query)
                    {
                        string output = String.Format("{0,-15} {1,15} {2,15} {3,15} \n", group.name, group.price, group.category, group.brand_name);
                        Console.WriteLine(output);

                    }
                }
                    else
                    {
                        var query = from pro in p
                                join br in b
                                on pro.brand_id equals br.id
                                orderby pro.name descending
                                select new { pro.name, pro.category, pro.price, br.brand_name };

                    foreach (var group in query)
                    {
                        string output = String.Format("{0,-15} {1,15} {2,15} {3,15} \n", group.name, group.price, group.category, group.brand_name);
                        Console.WriteLine(output);

                    }


                    }


              
            }

            else //order by price
            {
                if (y == 1)
                {
                    var query = from pro in p
                                join br in b
                                on pro.brand_id equals br.id
                                orderby pro.price ascending
                                select new { pro.name, pro.category, pro.price, br.brand_name };

                    foreach (var group in query)
                    {
                        string output = String.Format("{0,-15} {1,15} {2,15} {3,15} \n", group.name, group.price, group.category, group.brand_name);
                        Console.WriteLine(output);

                    }
                }
                else
                {
                    var query = from pro in p
                                join br in b
                                on pro.brand_id equals br.id
                                orderby pro.price descending
                                select new { pro.name, pro.category, pro.price, br.brand_name };

                    foreach (var group in query)
                    {
                        string output = String.Format("{0,-15} {1,15} {2,15} {3,15} \n", group.name, group.price, group.category, group.brand_name);
                        Console.WriteLine(output);

                    }


                }




            }


        }



       public void show_menu()
       {
           bool flag = true;
           while(flag)
           {
               Console.WriteLine(" 1- Show All Products " + "\t" + "2- Add Products " + "\t" + "3-Select products less than specific price" + "\t" + "4-List all brands" );
               Console.WriteLine("5-Sort and show products" + "\t" + "6-To exit");
               int choice;
               choice = int.Parse(Console.ReadLine());

               if (choice == 1) //show products
               { show_products(); }
               if (choice == 2) //add product
               {
                   string productname;
                   long price;
                   string categ;
                   string brandname;

                   Console.WriteLine("Enter please product name, price , category , brand name : ");
                   Console.WriteLine("ps : make sure the brand already exist before adding a new product");
                   productname = Console.ReadLine();
                   price = long.Parse(Console.ReadLine());
                   categ = Console.ReadLine();
                   brandname = Console.ReadLine();
                   add_product(productname, price, categ, brandname);
               }
               if (choice == 3) //select products less than specific price
               {
                   long price;
                   Console.WriteLine("Enter the price : ");
                   price = long.Parse(Console.ReadLine());
                   filter_product(price);

               }
               if (choice == 4) //list all brands
               { show_brands(); }
               if (choice == 5)
               {
                   Console.WriteLine("1-sort by name" + "\t" + "2-sort by price");
                   int val1 = int.Parse(Console.ReadLine());
                   Console.WriteLine("1-Ascending" + "\t" + "2-Descending");
                   int val2 = int.Parse(Console.ReadLine());
                   sort_products(val1, val2);
               }
               if (choice == 6) { flag = false; }


           }
           

       }
       
        

        static void Main(string[] args)
        {
            
            Program p = new Program();
            p.mappping();
           
            p.show_menu();
            Console.ReadKey();


        }
    }

}
