using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace StockTradeApi3.Models;

public partial class Db1Context : DbContext
{

    public Db1Context(DbContextOptions<Db1Context> options)
        : base(options)
    {
    }

    public virtual DbSet<BalanceCard> BalanceCards { get; set; }

    public virtual DbSet<BalanceTransaction> BalanceTransactions { get; set; }

    public virtual DbSet<CommissionRate> CommissionRates { get; set; }

    public virtual DbSet<Portfolio> Portfolios { get; set; }

    public virtual DbSet<Stock> Stocks { get; set; }

    public virtual DbSet<StockHistory> StockHistories { get; set; }

    public virtual DbSet<StockTransaction> StockTransactions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
    }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BalanceCard>(entity =>
        {
            entity.HasKey(e => e.CardId).HasName("PK__BalanceC__55FECD8ECFA2EC5D");

            entity.Property(e => e.CardId).HasColumnName("CardID");
            entity.Property(e => e.BalanceAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.BalanceCards)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__BalanceCa__UserI__72C60C4A");
        });

        modelBuilder.Entity<BalanceTransaction>(entity =>
        {
            entity.HasKey(e => e.BalanceTransactionId).HasName("PK__BalanceT__FDFBC5CEDED63217");

            entity.ToTable("BalanceTransaction");

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TransactionDate).HasColumnType("datetime");

            entity.HasOne(d => d.User).WithMany(p => p.BalanceTransactions)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__BalanceTr__UserI__3F466844");
        });

        modelBuilder.Entity<CommissionRate>(entity =>
        {
            entity.HasKey(e => e.RateId).HasName("PK__Commissi__58A7CCBC990F4166");

            entity.Property(e => e.RateId).HasColumnName("RateID");
            entity.Property(e => e.AdminCommissionRate).HasColumnType("decimal(5, 2)");
        });

        modelBuilder.Entity<Portfolio>(entity =>
        {
            entity.HasKey(e => e.PortfolioId).HasName("PK__Portfoli__6D3A139DD1848006");

            entity.Property(e => e.PortfolioId).HasColumnName("PortfolioID");
            entity.Property(e => e.BuyValue).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Stock).WithMany(p => p.Portfolios)
                .HasForeignKey(d => d.StockId)
                .HasConstraintName("FK_Portfolios_stocks");

            entity.HasOne(d => d.StockTransaction).WithMany(p => p.Portfolios)
                .HasForeignKey(d => d.StockTransactionId)
                .HasConstraintName("FK_Portfolios_stockTransaction");

            entity.HasOne(d => d.User).WithMany(p => p.Portfolios)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_Portfolios_Users");
        });

        modelBuilder.Entity<Stock>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__stocks__3213E83F7FACE6F3");

            entity.ToTable("stocks");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Buy)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("buy");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.IsActive)
                .HasDefaultValueSql("((1))")
                .HasColumnName("isActive");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.Sell)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("sell");
        });

        modelBuilder.Entity<StockHistory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__StockHis__3213E83FD41188EB");

            entity.ToTable("StockHistory");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Buy)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("buy");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Name)
                .HasMaxLength(10)
                .HasColumnName("name");
            entity.Property(e => e.Sell)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("sell");

            entity.HasOne(d => d.Stock).WithMany(p => p.StockHistories)
                .HasForeignKey(d => d.StockId)
                .HasConstraintName("FK_Stock");
        });

        modelBuilder.Entity<StockTransaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__StockTra__3213E83FCDE12FE1");

            entity.ToTable("StockTransaction");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TransactionDate).HasColumnType("datetime");
            entity.Property(e => e.Type)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("TYPE");

            entity.HasOne(d => d.Stock).WithMany(p => p.StockTransactions)
                .HasForeignKey(d => d.StockId)
                .HasConstraintName("FK__StockTran__Stock__4316F928");

            entity.HasOne(d => d.User).WithMany(p => p.StockTransactions)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__StockTran__UserI__3D5E1FD2");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__users__3213E83FB6BB9EF5");

            entity.ToTable("users");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Balance)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("balance");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Password).HasColumnName("password");
            entity.Property(e => e.Role)
                .HasMaxLength(255)
                .HasColumnName("role");
            entity.Property(e => e.Username)
                .HasMaxLength(255)
                .HasColumnName("username");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
