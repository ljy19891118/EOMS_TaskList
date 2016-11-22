using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;

namespace TaskList
{
    class Dao
    {
        public static string ConnSqlLiteDbPath = "data.db";


        public static SQLiteConnection Conn
        {
            get
            {
                SQLiteConnection conn = new SQLiteConnection();
                SQLiteConnectionStringBuilder connstr = new SQLiteConnectionStringBuilder();
                connstr.DataSource = ConnSqlLiteDbPath;
                conn.ConnectionString = connstr.ToString();
                conn.Open();
                return conn;
            }
        }

        internal static void TryInit()
        {
            try { Init(); }
            catch (Exception ex) { };
        }

        private static void Init()

        {
            SQLiteCommand command1 = Conn.CreateCommand();
            command1.CommandText = @"
                    create table task(
                        EID varchar PRIMARY KEY,
                        TaskId varchar,
                        Topic varchar,
                        Type varchar,
                        Description varchar,
                        HappenTime varchar,
                        T2Time varchar,
                        Progress varchar,
                        Status varchar,
                        UserName varchar,
                        IsArched varchar,
                        Location varchar
                    );
            ";
            command1.ExecuteNonQuery();
            SQLiteCommand command2 = Conn.CreateCommand();
            command2.CommandText = @"
                    create table user(
                        ID varchar PRIMARY KEY,
                        UserName varchar,
                        Password varchar,
                        GroupNo varchar,
                        Type varchar,
                        OnDuty varchar,
                        Next varchar,
                        Name varchar,
                        IsOK varchar
                    );
            ";
            command2.ExecuteNonQuery();

        }

        public static void Insert(Task data)
        {
            SQLiteCommand command = Conn.CreateCommand();
            command.CommandText = String.Format(@"
                   insert into task(
                        EID,
                        TaskId,
                        Topic,
                        Type,
                        Description,
                        HappenTime,
                        T2Time,
                        Progress,
                        Status,
                        UserName,
                        IsArched,
                        Location
                   ) values ('{0}', '{1}','{2}','{3}','{4}', '{5}','{6}','{7}','{8}','{9}','{10}','{11}');
            ", data.EID, data.TaskId, data.Topic, data.Type, data.Descrition, data.HappenTime, data.T2Time, data.Progress, data.Status, data.UserName, data.IsArched, data.Location);
            command.ExecuteNonQuery();
        }

        public static ObservableCollection<Task> SelectCtl()
        {
            ObservableCollection<Task> list = new ObservableCollection<Task>();

            SQLiteCommand command = Conn.CreateCommand();
            command.CommandText = String.Format(@"
                   select * from task where IsArched = '0' or IsArched = '' order by TaskID desc;
            ");
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                Task task = new Task();
                task.EID = reader.GetString(0);
                task.TaskId = reader.GetString(1);
                task.Topic = reader.GetString(2);
                task.Type = reader.GetString(3);
                task.Descrition = reader.GetString(4);
                task.HappenTime = reader.GetString(5);
                task.T2Time = reader.GetString(6);
                task.Progress = reader.GetString(7);
                task.Status = reader.GetString(8);
                task.UserName = reader.GetString(9);
                task.IsArched = reader.GetString(10);
                task.Location = reader.GetString(11);
                list.Add(task);

            }
            return list;
        }

        public static ObservableCollection<Task> SelectCtlByUser(string username)
        {
            ObservableCollection<Task> list = new ObservableCollection<Task>();

            SQLiteCommand command = Conn.CreateCommand();
            command.CommandText = String.Format(@"
                   select * from task where IsArched <> '1' and username = '{0}' order by TaskID desc;
            ", username);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                Task task = new Task();
                task.EID = reader.GetString(0);
                task.TaskId = reader.GetString(1);
                task.Topic = reader.GetString(2);
                task.Type = reader.GetString(3);
                task.Descrition = reader.GetString(4);
                task.HappenTime = reader.GetString(5);
                task.T2Time = reader.GetString(6);
                task.Progress = reader.GetString(7);
                task.Status = reader.GetString(8);
                task.UserName = reader.GetString(9);
                task.IsArched = reader.GetString(10);
                task.Location = reader.GetString(11);
                list.Add(task);

            }
            return list;
        }

        public static ObservableCollection<Task> SelectAtl()
        {
            ObservableCollection<Task> list = new ObservableCollection<Task>();

            SQLiteCommand command = Conn.CreateCommand();
            command.CommandText = String.Format(@"
                   select * from task where IsArched = '1' order by TaskID desc;
            ");
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                Task task = new Task();
                task.EID = reader.GetString(0);
                task.TaskId = reader.GetString(1);
                task.Topic = reader.GetString(2);
                task.Type = reader.GetString(3);
                task.Descrition = reader.GetString(4);
                task.HappenTime = reader.GetString(5);
                task.T2Time = reader.GetString(6);
                task.Progress = reader.GetString(7);
                task.Status = reader.GetString(8);
                task.UserName = reader.GetString(9);
                task.IsArched = reader.GetString(10);
                task.Location = reader.GetString(11);
                list.Add(task);

            }
            return list;
        }

        public static ObservableCollection<Task> SelectAtl(int pageSize, int pageNo)
        {
            ObservableCollection<Task> list = new ObservableCollection<Task>();

            SQLiteCommand command = Conn.CreateCommand();
            command.CommandText = String.Format(@"
                   select * from task where IsArched = '1' order by TaskID limit {0}, {1};
            ", pageNo * pageSize, pageSize);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                Task task = new Task();
                task.EID = reader.GetString(0);
                task.TaskId = reader.GetString(1);
                task.Topic = reader.GetString(2);
                task.Type = reader.GetString(3);
                task.Descrition = reader.GetString(4);
                task.HappenTime = reader.GetString(5);
                task.T2Time = reader.GetString(6);
                task.Progress = reader.GetString(7);
                task.Status = reader.GetString(8);
                task.UserName = reader.GetString(9);
                task.IsArched = reader.GetString(10);
                task.Location = reader.GetString(11);
                list.Add(task);

            }
            return list;
        }

        public static void ArchTask(string eid)
        {
            SQLiteCommand command = Conn.CreateCommand();
            command.CommandText = String.Format(@"
                   update task set IsArched = '1' where EID = '{0}';
            ", eid);
            command.ExecuteNonQuery();
        }

        public static void Update(Task task)
        {
            SQLiteCommand command = Conn.CreateCommand();
            command.CommandText = String.Format(@"
                   update task set Status = '{0}' and Progress = '{1}' where EID = '{2}';
            ", task.Status, task.Progress, task.EID);
            command.ExecuteNonQuery();
        }


        public static ObservableCollection<User> SelectOnDutyUsers()
        {
            string today = DateTime.Now.ToString("yyyy-MM-dd");
            ObservableCollection<User> list = new ObservableCollection<User>();

            SQLiteCommand update1 = Conn.CreateCommand();
            update1.CommandText = String.Format(@"
                   update user set OnDuty = '{0}' where ID in (select Next from user where OnDuty <> '' and OnDuty != '{1}');
            ", today, today);
            update1.ExecuteNonQuery();

            SQLiteCommand update2 = Conn.CreateCommand();
            update2.CommandText = String.Format(@"
                   update user set OnDuty = '' where OnDuty <> '' and OnDuty != '{0}';
            ", today);
            update2.ExecuteNonQuery();

            SQLiteCommand select = Conn.CreateCommand();
            select.CommandText = String.Format(@"
                   select * from user where OnDuty <> '' and IsOK = '1';
            ");
            SQLiteDataReader reader = select.ExecuteReader();
            while (reader.Read())
            {
                User user = new User();
                user.ID = reader.GetString(0);
                user.UserName = reader.GetString(1);
                user.Password = reader.GetString(2);
                user.GroupNo = reader.GetString(3);
                user.Type = reader.GetString(4);
                user.OnDuty = reader.GetString(5);
                user.Next = reader.GetString(6);
                user.Name = reader.GetString(7);
                user.IsOK = reader.GetString(8);
                list.Add(user);

            }


            return list;
        }

        public static ObservableCollection<User> SelectAllUsers()
        {
            ObservableCollection<User> list = new ObservableCollection<User>();

            SQLiteCommand command = Conn.CreateCommand();
            command.CommandText = String.Format(@"
                   select * from user;
            ");
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                User user = new User();
                user.ID = reader.GetString(0);
                user.UserName = reader.GetString(1);
                user.Password = reader.GetString(2);
                user.GroupNo = reader.GetString(3);
                user.Type = reader.GetString(4);
                user.OnDuty = reader.GetString(5);
                user.Next = reader.GetString(6);
                user.Name = reader.GetString(7);
                user.IsOK = reader.GetString(8);
                list.Add(user);

            }
            return list;
        }

        public static User SelectUser(string userName)
        {
            User user = null;
            SQLiteCommand command = Conn.CreateCommand();
            command.CommandText = String.Format(@"
                   select * from user where UserName = '{0}';
            ", userName);
            SQLiteDataReader reader = command.ExecuteReader();
            if (reader.Read())
            {
                user = new User();
                user.ID = reader.GetString(0);
                user.UserName = reader.GetString(1);
                user.Password = reader.GetString(2);
                user.GroupNo = reader.GetString(3);
                user.Type = reader.GetString(4);
                user.OnDuty = reader.GetString(5);
                user.Next = reader.GetString(6);
                user.Name = reader.GetString(7);
                user.IsOK = reader.GetString(8);
                reader.Close();
            }
            return user;
        }

        public static User SelectUserByID(string id)
        {
            User user = null;
            SQLiteCommand command = Conn.CreateCommand();
            command.CommandText = String.Format(@"
                   select * from user where ID = '{0}';
            ", id);
            SQLiteDataReader reader = command.ExecuteReader();
            if (reader.Read())
            {
                user = new User();
                user.ID = reader.GetString(0);
                user.UserName = reader.GetString(1);
                user.Password = reader.GetString(2);
                user.GroupNo = reader.GetString(3);
                user.Type = reader.GetString(4);
                user.OnDuty = reader.GetString(5);
                user.Next = reader.GetString(6);
                user.Name = reader.GetString(7);
                user.IsOK = reader.GetString(8);
                reader.Close();
            }
            return user;
        }

        public static int Save(Task task)
        {
            if (SelectTask(task.EID) == null)
            {
                Insert(task);
                return 0;
            }
            else
            {
                Update(task);
                return 1;
            }
        }

        public static Task SelectTask(string eid)
        {
            Task task = null;
            SQLiteCommand command = Conn.CreateCommand();
            command.CommandText = String.Format(@"
                   select * from task where EID = '{0}';
            ", eid);
            SQLiteDataReader reader = command.ExecuteReader();
            if (reader.Read())
            {
                task = new Task();
                task.EID = reader.GetString(0);
                task.TaskId = reader.GetString(1);
                task.Topic = reader.GetString(2);
                task.Type = reader.GetString(3);
                task.Descrition = reader.GetString(4);
                task.HappenTime = reader.GetString(5);
                task.T2Time = reader.GetString(6);
                task.Progress = reader.GetString(7);
                task.Status = reader.GetString(8);
                task.UserName = reader.GetString(9);
                task.IsArched = reader.GetString(10);
                task.Location = reader.GetString(11);
                reader.Close();
            }
            return task;
        }

        public static void Save(User user)
        {
            // 先判断要更新还是要新增
            if (user.ID == null)
            {
                user.ID = DateTime.Now.ToFileTime().ToString();
                using (SQLiteCommand query = Conn.CreateCommand())
                {
                    query.CommandText = String.Format(@"
                        select max(ID) from user where GroupNo = '{0}' and Type = '{1}' and IsOK ='1';
                    ", user.GroupNo, user.Type);
                    SQLiteDataReader reader = query.ExecuteReader();
                    // 搜不到或者搜到的值为null则说明是本组本类型第一个
                    if (reader.Read())
                    {
                        string maxID = reader.GetValue(0).ToString();
                        reader.Close();
                        if (maxID == null || maxID == "")
                        {
                            user.OnDuty = GetToday();
                            user.Next = user.ID;
                        }
                        else
                        {
                            User maxUser = SelectUserByID(maxID);
                            user.Next = maxUser.Next;
                            using (SQLiteCommand update = Conn.CreateCommand())
                            {
                                update.CommandText = String.Format(@"
                                    update user set Next = '{0}' where ID = '{1}';
                                ", user.ID, maxUser.ID);
                                update.ExecuteNonQuery();
                            }
                        }
                    }
                    else
                    {
                        user.OnDuty = GetToday();
                        user.Next = user.ID;
                    }

                }
                SQLiteCommand insert = Conn.CreateCommand();
                insert.CommandText = String.Format(@"
                   insert into user(
                        ID,
                        UserName,
                        Password,
                        GroupNo,
                        Type,
                        OnDuty,
                        Next,
                        Name,
                        IsOK
                   ) values ('{0}', '{1}','{2}','{3}','{4}', '{5}','{6}','{7}','{8}');
            ", user.ID, user.UserName, user.Password, user.GroupNo, user.Type, user.OnDuty, user.Next, user.Name, "1");
                insert.ExecuteNonQuery();
            }
            else
            {
                // 先查看Type是否发生改变
                using (SQLiteCommand query = Conn.CreateCommand())
                {
                    query.CommandText = String.Format(@"
                        select Type from user where ID ='{0}';
                    ", user.ID);
                    SQLiteDataReader reader = query.ExecuteReader();
                    if (reader.Read())
                    {
                        string type = reader.GetString(0);
                        reader.Close();
                        if(type != user.Type)
                        {
                            Delete(user);
                            user.ID = null;
                            Save(user);
                            return;
                        }
                    }
                    else
                    {
                        user.ID = null;
                        Save(user);
                        return;
                    }
                }
                // 再查看有效性
                using (SQLiteCommand query = Conn.CreateCommand())
                {
                    query.CommandText = String.Format(@"
                        select IsOK from user where ID ='{0}';
                    ", user.ID);
                    SQLiteDataReader reader = query.ExecuteReader();
                    if (reader.Read())
                    {
                        string IsOK = reader.GetString(0);
                        reader.Close();
                        if (IsOK == "1")
                        {
                            SQLiteCommand update = Conn.CreateCommand();
                            update.CommandText = String.Format(@"
                                update user set
                                   UserName = '{0}',
                                   Password = '{1}',
                                   GroupNo = '{2}',
                                   Type = '{3}',                   
                                   Name = '{4}'
                                where ID = '{5}';
                            ", user.UserName, user.Password, user.GroupNo, user.Type, user.Name, user.ID);
                            update.ExecuteNonQuery();
                        }
                        else
                        {
                            using (SQLiteCommand update = Conn.CreateCommand())
                            {
                                update.CommandText = String.Format(@"
                                update user set
                                   UserName = '{0}',
                                   Password = '{1}',
                                   GroupNo = '{2}',
                                   Type = '{3}',                   
                                   Name = '{4}',
                                   IsOK = '1',
                                where ID = '{5}';
                            ", user.UserName, user.Password, user.GroupNo, user.Type, user.Name, user.ID);
                                update.ExecuteNonQuery();
                            }
                            using (SQLiteCommand update = Conn.CreateCommand())
                            {
                                update.CommandText = String.Format(@"
                                    update user set Next = '{0}' where ID <> '{1}' and Next = '{2}';
                                ", user.ID, user.ID, user.Next);
                                update.ExecuteNonQuery();
                            }
                        }
                    }
                    else
                    {
                        user.ID = null;
                        Save(user);
                        return;
                    }

                }


            }
        }


        public static void Delete(User user)
        {

            // 查找user目前的值班状态
            SQLiteCommand query = Conn.CreateCommand();
            query.CommandText = String.Format(@"
                   select IsOK, OnDuty from user where ID = '{0}';
            ", user.ID);
            SQLiteDataReader reader = query.ExecuteReader();
            if (reader.Read())
            {
                string isOK = reader.GetString(0);
                string onDuty = reader.GetString(1);
                reader.Close();
                if (isOK != "1")
                {
                    using (SQLiteCommand delete = Conn.CreateCommand())
                    {
                        delete.CommandText = String.Format(@"
                       delete from user where ID = '{0}';
                ", user.ID);
                        delete.ExecuteNonQuery();
                    }
                    return;
                }
                else
                {
                    string today = GetToday();
                    if (onDuty != "" && onDuty == today)
                    {
                        using (SQLiteCommand update = Conn.CreateCommand())
                        {
                            update.CommandText = String.Format(@"
                            update user set Onduty = '{0}' where ID = '{1}';
                    ", today, user.Next);
                            update.ExecuteNonQuery();
                        }
                    }
                }
            }
            else
            {
                return;
            }

            using (SQLiteCommand update = Conn.CreateCommand())
            {
                update.CommandText = String.Format(@"
                            update user set Next = '{0}' where Next = '{1}';
                    ", user.Next, user.ID);
                update.ExecuteNonQuery();
            }

            using (SQLiteCommand delete = Conn.CreateCommand())
            {
                delete.CommandText = String.Format(@"
                       delete from user where ID = '{0}';
                ", user.ID);
                delete.ExecuteNonQuery();
            }
        }

        public static void Invaild(User user)
        {
            // 查找user目前的值班状态
            SQLiteCommand query = Conn.CreateCommand();
            query.CommandText = String.Format(@"
                   select OnDuty from user where ID = '{0}';
            ", user.ID);
            SQLiteDataReader reader = query.ExecuteReader();
            if (reader.Read())
            {
                string onDuty = reader.GetString(0);
                reader.Close();
                string today = GetToday();
                if (onDuty != "" && onDuty == today)
                {

                    using (SQLiteCommand update = Conn.CreateCommand())
                    {
                        update.CommandText = String.Format(@"
                            update user set Onduty = '{0}' where ID = '{1}';
                    ", today, user.Next);
                        update.ExecuteNonQuery();
                    }
                }
            }
            else
            {
                return;
            }

            using (SQLiteCommand update = Conn.CreateCommand())
            {
                update.CommandText = String.Format(@"
                            update user set Next = '{0}' where Next = '{1}';
                    ", user.Next, user.ID);
                update.ExecuteNonQuery();
            }

            using (SQLiteCommand update = Conn.CreateCommand())
            {
                update.CommandText = String.Format(@"
                       update user set IsOK = '0' where ID = '{0}';
                ", user.ID);
                update.ExecuteNonQuery();
            }

        }

        public static string GetToday()
        {
            return DateTime.Now.ToString("yyyy-MM-dd");
        }

    }
}
