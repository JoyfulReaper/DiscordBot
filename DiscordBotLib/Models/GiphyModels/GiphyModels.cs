using System.Collections.Generic;

namespace DiscordBotLib.Models.GiphyModels
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class Original
    {
        public string height { get; set; }
        public string width { get; set; }
        public string size { get; set; }
        public string url { get; set; }
        public string mp4_size { get; set; }
        public string mp4 { get; set; }
        public string webp_size { get; set; }
        public string webp { get; set; }
        public string frames { get; set; }
        public string hash { get; set; }
    }

    public class Downsized
    {
        public string height { get; set; }
        public string width { get; set; }
        public string size { get; set; }
        public string url { get; set; }
    }

    public class DownsizedLarge
    {
        public string height { get; set; }
        public string width { get; set; }
        public string size { get; set; }
        public string url { get; set; }
    }

    public class DownsizedMedium
    {
        public string height { get; set; }
        public string width { get; set; }
        public string size { get; set; }
        public string url { get; set; }
    }

    public class DownsizedSmall
    {
        public string height { get; set; }
        public string width { get; set; }
        public string mp4_size { get; set; }
        public string mp4 { get; set; }
    }

    public class DownsizedStill
    {
        public string height { get; set; }
        public string width { get; set; }
        public string size { get; set; }
        public string url { get; set; }
    }

    public class FixedHeight
    {
        public string height { get; set; }
        public string width { get; set; }
        public string size { get; set; }
        public string url { get; set; }
        public string mp4_size { get; set; }
        public string mp4 { get; set; }
        public string webp_size { get; set; }
        public string webp { get; set; }
    }

    public class FixedHeightDownsampled
    {
        public string height { get; set; }
        public string width { get; set; }
        public string size { get; set; }
        public string url { get; set; }
        public string webp_size { get; set; }
        public string webp { get; set; }
    }

    public class FixedHeightSmall
    {
        public string height { get; set; }
        public string width { get; set; }
        public string size { get; set; }
        public string url { get; set; }
        public string mp4_size { get; set; }
        public string mp4 { get; set; }
        public string webp_size { get; set; }
        public string webp { get; set; }
    }

    public class FixedHeightSmallStill
    {
        public string height { get; set; }
        public string width { get; set; }
        public string size { get; set; }
        public string url { get; set; }
    }

    public class FixedHeightStill
    {
        public string height { get; set; }
        public string width { get; set; }
        public string size { get; set; }
        public string url { get; set; }
    }

    public class FixedWidth
    {
        public string height { get; set; }
        public string width { get; set; }
        public string size { get; set; }
        public string url { get; set; }
        public string mp4_size { get; set; }
        public string mp4 { get; set; }
        public string webp_size { get; set; }
        public string webp { get; set; }
    }

    public class FixedWidthDownsampled
    {
        public string height { get; set; }
        public string width { get; set; }
        public string size { get; set; }
        public string url { get; set; }
        public string webp_size { get; set; }
        public string webp { get; set; }
    }

    public class FixedWidthSmall
    {
        public string height { get; set; }
        public string width { get; set; }
        public string size { get; set; }
        public string url { get; set; }
        public string mp4_size { get; set; }
        public string mp4 { get; set; }
        public string webp_size { get; set; }
        public string webp { get; set; }
    }

    public class FixedWidthSmallStill
    {
        public string height { get; set; }
        public string width { get; set; }
        public string size { get; set; }
        public string url { get; set; }
    }

    public class FixedWidthStill
    {
        public string height { get; set; }
        public string width { get; set; }
        public string size { get; set; }
        public string url { get; set; }
    }

    public class Looping
    {
        public string mp4_size { get; set; }
        public string mp4 { get; set; }
    }

    public class OriginalStill
    {
        public string height { get; set; }
        public string width { get; set; }
        public string size { get; set; }
        public string url { get; set; }
    }

    public class OriginalMp4
    {
        public string height { get; set; }
        public string width { get; set; }
        public string mp4_size { get; set; }
        public string mp4 { get; set; }
    }

    public class Preview
    {
        public string height { get; set; }
        public string width { get; set; }
        public string mp4_size { get; set; }
        public string mp4 { get; set; }
    }

    public class PreviewGif
    {
        public string height { get; set; }
        public string width { get; set; }
        public string size { get; set; }
        public string url { get; set; }
    }

    public class PreviewWebp
    {
        public string height { get; set; }
        public string width { get; set; }
        public string size { get; set; }
        public string url { get; set; }
    }

    public class _480wStill
    {
        public string height { get; set; }
        public string width { get; set; }
        public string size { get; set; }
        public string url { get; set; }
    }

    public class Images
    {
        public Original original { get; set; }
        public Downsized downsized { get; set; }
        public DownsizedLarge downsized_large { get; set; }
        public DownsizedMedium downsized_medium { get; set; }
        public DownsizedSmall downsized_small { get; set; }
        public DownsizedStill downsized_still { get; set; }
        public FixedHeight fixed_height { get; set; }
        public FixedHeightDownsampled fixed_height_downsampled { get; set; }
        public FixedHeightSmall fixed_height_small { get; set; }
        public FixedHeightSmallStill fixed_height_small_still { get; set; }
        public FixedHeightStill fixed_height_still { get; set; }
        public FixedWidth fixed_width { get; set; }
        public FixedWidthDownsampled fixed_width_downsampled { get; set; }
        public FixedWidthSmall fixed_width_small { get; set; }
        public FixedWidthSmallStill fixed_width_small_still { get; set; }
        public FixedWidthStill fixed_width_still { get; set; }
        public Looping looping { get; set; }
        public OriginalStill original_still { get; set; }
        public OriginalMp4 original_mp4 { get; set; }
        public Preview preview { get; set; }
        public PreviewGif preview_gif { get; set; }
        public PreviewWebp preview_webp { get; set; }
        public _480wStill _480w_still { get; set; }
    }

    public class User
    {
        public string avatar_url { get; set; }
        public string banner_image { get; set; }
        public string banner_url { get; set; }
        public string profile_url { get; set; }
        public string username { get; set; }
        public string display_name { get; set; }
        public string description { get; set; }
        public string instagram_url { get; set; }
        public string website_url { get; set; }
        public bool is_verified { get; set; }
    }

    public class Onload
    {
        public string url { get; set; }
    }

    public class Onclick
    {
        public string url { get; set; }
    }

    public class Onsent
    {
        public string url { get; set; }
    }

    public class Analytics
    {
        public Onload onload { get; set; }
        public Onclick onclick { get; set; }
        public Onsent onsent { get; set; }
    }

    public class Datum
    {
        public string type { get; set; }
        public string id { get; set; }
        public string url { get; set; }
        public string slug { get; set; }
        public string bitly_gif_url { get; set; }
        public string bitly_url { get; set; }
        public string embed_url { get; set; }
        public string username { get; set; }
        public string source { get; set; }
        public string title { get; set; }
        public string rating { get; set; }
        public string content_url { get; set; }
        public string source_tld { get; set; }
        public string source_post_url { get; set; }
        public int is_sticker { get; set; }
        public string import_datetime { get; set; }
        public string trending_datetime { get; set; }
        public Images images { get; set; }
        public User user { get; set; }
        public string analytics_response_payload { get; set; }
        public Analytics analytics { get; set; }
    }

    public class Pagination
    {
        public int total_count { get; set; }
        public int count { get; set; }
        public int offset { get; set; }
    }

    public class Meta
    {
        public int status { get; set; }
        public string msg { get; set; }
        public string response_id { get; set; }
    }

    public class GiphyRoot
    {
        public List<Datum> data { get; set; }
        public Pagination pagination { get; set; }
        public Meta meta { get; set; }
    }
}