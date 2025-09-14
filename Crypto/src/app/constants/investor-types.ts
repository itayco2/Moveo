export interface InvestorType {
  value: string;
  name: string;
  description: string;
}

export const INVESTOR_TYPES: InvestorType[] = [
  {
    value: 'HODLer',
    name: 'HODLer',
    description: 'Long-term investor who believes in holding crypto assets'
  },
  {
    value: 'Day Trader',
    name: 'Day Trader',
    description: 'Active trader looking for short-term opportunities'
  },
  {
    value: 'NFT Collector',
    name: 'NFT Collector',
    description: 'Interested in digital art and collectibles'
  },
  {
    value: 'DeFi Enthusiast',
    name: 'DeFi Enthusiast',
    description: 'Focused on decentralized finance protocols'
  }
];
