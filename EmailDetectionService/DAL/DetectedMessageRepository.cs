﻿using EmailDetectionService.Models;
using log4net;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace EmailDetectionService.DAL
{
    public class DetectedMessageRepository : IDataSourceDetectedMessages
    {
        private static string CONNECTION_STRING_NAME = "DefaultConnection";
        private static readonly ILog _log = LogManager.GetLogger(typeof(DetectedMessageRepository));

        public List<MessageModel> GetMessages(int maxCount = 100)
        {
            List<MessageModel> messages = new List<MessageModel>();

            using (SqlConnection conn = new SqlConnection(Config.GetConnectionString(CONNECTION_STRING_NAME)))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = @"SELECT * FROM 
	                                     (SELECT TOP (@Count) * 
	                                      FROM [dbo].[DetectedMessages]
	                                      ORDER BY [ID] DESC) AS Result
                                        ORDER BY Result.ID ASC";

                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@Count", maxCount);
                    cmd.Connection = conn;

                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        try
                        {
                            while (reader.Read())
                            {
                                MessageModel message = new MessageModel();
                                message.Message_ID = reader.GetString(1);
                                message.Subject = reader.GetString(2);
                                message.From = reader.GetString(3);
                                message.To = reader.GetString(4);
                                message.Date = reader.GetString(5);

                                messages.Add(message);
                            }
                        }
                        catch (Exception ex)
                         {
                            _log.Error("Database error: " + ex.ToString() + ex.Data.ToString());
                            throw;
                        }
                    }
                }
            }

            return messages;
        }

        public bool InsertMessage(MessageModel message)
        {
            using (SqlConnection conn = new SqlConnection(Config.GetConnectionString(CONNECTION_STRING_NAME)))
            {
                using (SqlCommand cmd = new SqlCommand("DetectedMessageAdd", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@MessageId", message.Message_ID);
                    cmd.Parameters.AddWithValue("@Subject", message.Subject);
                    cmd.Parameters.AddWithValue("@From", message.From);
                    cmd.Parameters.AddWithValue("@To", message.To);
                    cmd.Parameters.AddWithValue("@Date", message.Date);

                    try
                    {
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                    catch (SqlException ex)
                    {
                        _log.Error("Database error: " + ex.ToString() + ex.Data.ToString());
                        return false;
                    }
                }
            }

            return true;
        }

        public bool InsertMessagesScope(List<MessageModel> messages)
        {
            DataTable detectedEmailList = new DataTable("DetectedEmailList");
            detectedEmailList.Columns.Add("MessageId", typeof(string));
            detectedEmailList.Columns.Add("Subject", typeof(string));
            detectedEmailList.Columns.Add("From", typeof(string));
            detectedEmailList.Columns.Add("To", typeof(string));
            detectedEmailList.Columns.Add("Date", typeof(string));

            foreach (MessageModel msg in messages)
            {
                detectedEmailList.Rows.Add(msg.Message_ID, msg.Subject, msg.From, msg.To, msg.Date);
            }

            using (SqlConnection conn = new SqlConnection(Config.GetConnectionString(CONNECTION_STRING_NAME)))
            {
                using (SqlCommand cmd = new SqlCommand("DetectedMessageAddRange", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@EmailList", detectedEmailList);

                    try
                    {
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                    catch (SqlException ex)
                    {
                        _log.Error("Database error: " + ex.ToString() + ex.Data.ToString());
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
