export interface DashboardResponse {
  news: NewsItem[];
  prices: CoinPrice[];
  aiInsight: AiInsight | null;
  meme: Meme | null;
}

export interface NewsItem {
  id: string;
  title: string;
  url: string;
  source: string;
  publishedAt: string;
  tags: string[];
  userFeedback?: number | null;
}

export interface CoinPrice {
  id: string;
  symbol: string;
  name: string;
  currentPrice: number;
  priceChange24h: number;
  priceChangePercentage24h: number;
  image: string;
  userFeedback?: number | null;
}

export interface AiInsight {
  id: string;
  title: string;
  content: string;
  tags: string[];
  generatedAt: string;
  userFeedback?: number | null;
}

export interface Meme {
  id: string;
  title: string;
  imageUrl: string;
  source: string;
  userFeedback?: number | null;
}

export interface FeedbackRequest {
  contentType: string;
  contentId: string;
  isPositive: boolean;
}



