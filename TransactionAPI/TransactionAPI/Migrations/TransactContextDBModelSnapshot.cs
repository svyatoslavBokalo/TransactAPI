﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TransactionAPI.ContextDB;

#nullable disable

namespace TransactionAPI.Migrations
{
    [DbContext(typeof(TransactContextDB))]
    partial class TransactContextDBModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.7")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("TransactionAPI.Models.TransactionModel", b =>
                {
                    b.Property<string>("TransactionId")
                        .HasColumnType("nvarchar(450)")
                        .HasColumnName("transaction_id");

                    b.Property<decimal?>("Amount")
                        .HasColumnType("decimal(18,2)")
                        .HasColumnName("amount");

                    b.Property<string>("ClientLocation")
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("client_location");

                    b.Property<string>("Email")
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("email");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("name");

                    b.Property<DateTime>("TransactionDate")
                        .HasColumnType("datetime2")
                        .HasColumnName("transaction_date");

                    b.HasKey("TransactionId");

                    b.ToTable("Transactions");
                });
#pragma warning restore 612, 618
        }
    }
}
