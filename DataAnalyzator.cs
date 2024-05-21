using System.Globalization;

namespace Staking;

public class DataAnalyzer
{
    public event EventHandler<decimal>? OnTokenPriceTooHigh;

    public event EventHandler<decimal>? OnTokenPriceTooLow;

    public event EventHandler? OnPositionsDisbalanced;

    public event EventHandler<(decimal, decimal)>? OnShortTooExpensive;

    public event EventHandler<(decimal, decimal)>? OnShortBackToNormal;

    public event EventHandler? OnTokenShortAndStakedDisbalanced;

    public AggregatedData Data { get; set; }

    public DataAnalyzer(AggregatedData data, IConfiguration config)
	{
        Data = data;
    }

    public void Run()
    {
        CheckProfitability();

        Thread.Sleep(100);

        CheckIfStakeIsTooHigh();

        Thread.Sleep(100);

        CheckTokenStakedAndShortedAmount();
        
        Thread.Sleep(100);
        
        CheckIfShortIsTooHigh();
    }
    
    public void RunOld()
    {
        CheckProfitability();

        Thread.Sleep(100);

        CheckPositionsBalancing();

        Thread.Sleep(100);

        // CheckTokenPriceChange();
        //
        // Thread.Sleep(100);

        CheckTokenStakedAndShortedAmount();
    }

    private void CheckTokenPriceChange()
    {
        if (Data.CurrentTokenPrice > (Data.BaseTokenPrice * (1 + Data.TokenPriceThreshold)))
            OnTokenPriceTooHigh?.Invoke(this, Data.CurrentTokenPrice);

        else if (Data.CurrentTokenPrice < (Data.BaseTokenPrice * (1 - Data.TokenPriceThreshold)))
            OnTokenPriceTooLow?.Invoke(this, Data.CurrentTokenPrice);
    }

    private void CheckPositionsBalancing()
    {
        //Get Staking and Short Position USD Values
        var stakeValuation = Data.CurrentTokenPrice * Data.CurrentlyStaked;
        var shortValuation = Data.ShortPositionValue;

        //Calculate Total USD Value and targets for each  position
        var totalValue = Data.CalculateTotalValue();
        var stakingTarget = totalValue * 2 / 3;
        var shortTarget = totalValue * 1 / 3;

        //Check if Short position USD Value is significantly out of target
        var shortOutOfTarget = 
            shortValuation < shortTarget * (1 - Data.BalancerThreshold)
            || shortValuation > shortTarget * (1 + Data.BalancerThreshold);

        //Check if Staking USD Value is significantly out of target
        var stakingOutOfTarget = 
            stakeValuation < stakingTarget * (1 - Data.BalancerThreshold)
            || stakeValuation > stakingTarget * (1 + Data.BalancerThreshold);
        
        //If any of the upper conditions are true - start Balancer module
        if(shortOutOfTarget || stakingOutOfTarget)
            OnPositionsDisbalanced?.Invoke(this, EventArgs.Empty);
    }

    private void CheckIfShortIsTooHigh()
    {
        var shortValuation = Data.ShortPositionValue;
        
        var totalValue = Data.CalculateTotalValue();
        var shortTarget = totalValue * 1 / 3;
        
        var shortTooHigh = 
            shortValuation > shortTarget * (1 + Data.BalancerThreshold);
        
        if(shortTooHigh)
            OnTokenPriceTooLow?.Invoke(this, Data.CurrentTokenPrice);
    }

    private void CheckIfStakeIsTooHigh()
    {
        var stakeValuation = Data.CurrentTokenPrice * Data.CurrentlyStaked;
        
        var totalValue = Data.CalculateTotalValue();
        var stakingTarget = totalValue * 2 / 3;
        
        var stakingTooHigh = stakeValuation > stakingTarget * (1 + Data.BalancerThreshold);
        
        if(stakingTooHigh)
            OnTokenPriceTooHigh?.Invoke(this, Data.CurrentTokenPrice);
    }

    private void CheckProfitability()
    {
        var profitability = Data.CalculateProfitability();

        switch (profitability)
        {
            // case < 0.01m:
            //     OnShortTooExpensive?.Invoke(this, (Data.ShortFeesYearly, Data.StakeApr));
            //     break;
            // case >= 0.02m:
            //     OnShortBackToNormal?.Invoke(this, (Data.ShortFeesYearly, Data.StakeApr));
            //     break;
        }
    }

    private void CheckTokenStakedAndShortedAmount()
    {
        if(Data.ShortPositionInfo is null)
            return;
        
        var shortedInPosition = -Data.ShortPositionInfo.PositionAmt;
        
        var disbalance = Math.Abs(Data.CurrentlyStaked - shortedInPosition);
        
        if(disbalance > Data.BalancerBasedOnTokenThreshold)
            OnTokenShortAndStakedDisbalanced?.Invoke(this, EventArgs.Empty);
    }
}

